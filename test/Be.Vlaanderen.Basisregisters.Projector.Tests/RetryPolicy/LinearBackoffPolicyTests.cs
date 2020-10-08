namespace Be.Vlaanderen.Basisregisters.Projector.Tests.RetryPolicy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using ConnectedProjections;
    using FluentAssertions;
    using Infrastructure;
    using Infrastructure.Extensions;
    using Internal;
    using Internal.RetryPolicies;
    using Internal.StreamGapStrategies;
    using Microsoft.Extensions.Logging;
    using Moq;
    using SqlStreamStore.Streams;
    using Xunit;

    public class WhenApplyingALinearBackoffPolicyOnAHandler
    {
        private readonly MessageHandlerWithExecutionTracking _handlerWithoutPolicy;
        private readonly IFixture _fixture;
        private readonly IConnectedProjectionMessageHandler _sut;
        private readonly Times _numberOfExpectedAttempts;

        public WhenApplyingALinearBackoffPolicyOnAHandler()
        {
            _fixture = new Fixture()
                .CustomizeConnectedProjectionNames();

            _handlerWithoutPolicy = new MessageHandlerWithExecutionTracking(
                _fixture.Create<ConnectedProjectionName>(),
                new Mock<ILogger>().Object);

            var numberOfRetries = _fixture.CreatePositive<int>() + 1;
            _numberOfExpectedAttempts = Times.Once();
            var initialWait = TimeSpan.FromMilliseconds(_fixture.CreatePositive<int>());
            var linearBackoffPolicy = new LinearBackOff<RetryException>(numberOfRetries, initialWait);

            _sut = linearBackoffPolicy.ApplyOn(_handlerWithoutPolicy);
        }

        [Fact]
        public void ThenTheHandlerLoggerIsUnchanged()
        {
            _sut.Logger
                .Should()
                .BeSameAs(_handlerWithoutPolicy.Logger);
        }

        [Fact]
        public void ThenProjectionRunnerNameIsTheSameAsTheOriginal()
        {
            _sut.RunnerName
                .Should()
                .BeSameAs(_handlerWithoutPolicy.RunnerName);
        }

        [Fact]
        public async Task ThenHandleActionCallsTheHandlerWithoutAPolicy()
        {
            var messages = _fixture.CreateMany<StreamMessage>().ToList();
            var token = new CancellationToken();

            await _sut.HandleAsync(messages, Mock.Of<IStreamGapStrategy>(), token);
            _handlerWithoutPolicy.VerifyExecuted(messages, _numberOfExpectedAttempts);
        }
    }

    public class WhenMessageHandlerThrowsAnExceptionThatWasNotDefinedToRetry
    {
        private readonly MessageHandlerWithExecutionTracking _handlerWithoutPolicy;
        private readonly IConnectedProjectionMessageHandler _sut;
        private readonly IReadOnlyCollection<StreamMessage> _messages;

        public WhenMessageHandlerThrowsAnExceptionThatWasNotDefinedToRetry()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionNames();

            _handlerWithoutPolicy = new MessageHandlerWithExecutionTracking(
               fixture.Create<ConnectedProjectionName>(),
               new FakeLoggerFactory().ResolveLoggerMock<MessageHandlerWithExecutionTracking>().AsLogger(),
               new DoNotRetryException());

            var numberOfRetries = fixture.CreatePositive<int>() + 1;
            var initialWait = TimeSpan.FromMilliseconds(fixture.CreatePositive<int>());
            _messages = fixture.CreateMany<StreamMessage>().ToList();

            _sut = new LinearBackOff<RetryException>(numberOfRetries, initialWait)
                .ApplyOn(_handlerWithoutPolicy);
        }

        private async Task Act() => await _sut.HandleAsync(_messages, Mock.Of<IStreamGapStrategy>(), CancellationToken.None);

        [Fact]
        public void ThenTheExceptionIsNotCaught()
        {
            ((Func<Task>)Act)
                .Should()
                .Throw<DoNotRetryException>();
        }

        [Fact]
        public async Task ThenHandleActionCallsTheHandlerWithoutAPolicy()
        {
            try
            {
                await Act();
            }
            catch (DoNotRetryException)
            { }

            _handlerWithoutPolicy.VerifyExecuted(_messages, Times.Once());
        }
    }

    public class WhenMessageHandlerThrowsAnExceptionThatWasNotDefinedToRetryAfterRetrying
    {
        private readonly MessageHandlerWithExecutionTracking _handlerWithoutPolicy;
        private readonly IConnectedProjectionMessageHandler _sut;
        private readonly IReadOnlyCollection<StreamMessage> _messages;
        private readonly FakeLogger _loggerMock;
        private readonly ConnectedProjectionName _projectionName;
        private readonly TimeSpan _initialWait;
        private readonly Times _numberOfExpectedAttempts;
        private readonly Exception[] _exceptionSequence;

        public WhenMessageHandlerThrowsAnExceptionThatWasNotDefinedToRetryAfterRetrying()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionNames();

            _exceptionSequence = new Exception[] { new RetryException(), new DoNotRetryException() };

            _loggerMock = new FakeLoggerFactory().ResolveLoggerMock<MessageHandlerWithExecutionTracking>();
            _projectionName = fixture.Create<ConnectedProjectionName>();
            _handlerWithoutPolicy = new MessageHandlerWithExecutionTracking(
               _projectionName,
               _loggerMock.AsLogger(),
               _exceptionSequence);

            var numberOfRetries = _exceptionSequence.Count(exception => exception is RetryException);
            _numberOfExpectedAttempts = Times.Exactly(1 + numberOfRetries);
            _initialWait = TimeSpan.FromMilliseconds(fixture.CreatePositive<int>());
            _messages = fixture.CreateMany<StreamMessage>().ToList();

            _sut = new LinearBackOff<RetryException>(numberOfRetries, _initialWait)
                .ApplyOn(_handlerWithoutPolicy);
        }

        private async Task Act() => await _sut.HandleAsync(_messages, Mock.Of<IStreamGapStrategy>(), CancellationToken.None);

        [Fact]
        public void ThenTheExceptionIsNotCaught()
        {
            ((Func<Task>)Act)
                .Should()
                .Throw<DoNotRetryException>();
        }

        [Fact]
        public async Task ThenHandleActionCallsTheHandlerWithoutAPolicyForEachAttempt()
        {
            try
            {
                await Act();
            }
            catch (DoNotRetryException)
            { }

            _handlerWithoutPolicy.VerifyExecuted(_messages, _numberOfExpectedAttempts);
        }
        
        [Fact]
        public async Task ThenAWarningIsLoggedForEachRetry()
        {
            try
            {
                await Act();
            }
            catch (DoNotRetryException)
            { }

            foreach (var attempt in _exceptionSequence.Select((exception, i) => new { Error = exception, Retry = i + 1 }))
            {
                if (attempt.Error is DoNotRetryException)
                    break;

                _loggerMock.Verify(
                    LogLevel.Warning,
                    $"Projection '{_projectionName}' failed. Retry attempt #{attempt.Retry} in {_initialWait.Multiply(attempt.Retry).TotalSeconds} seconds.",
                    Times.Once);
            }
        }
    }

    public class WhenMessageHandlerThrowsTheExceptionDefinedInThePolicyMoreThanTheNumberOfRetries
    {
        private readonly MessageHandlerWithExecutionTracking _handlerWithoutPolicy;
        private readonly IConnectedProjectionMessageHandler _sut;
        private readonly IReadOnlyCollection<StreamMessage> _messages;
        private readonly int _numberOfRetries;
        private readonly FakeLogger _loggerMock;
        private readonly ConnectedProjectionName _projectionName;
        private readonly TimeSpan _initialWait;
        private readonly Times _numberOfExpectedAttempts;

        public WhenMessageHandlerThrowsTheExceptionDefinedInThePolicyMoreThanTheNumberOfRetries()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionNames();

            var exceptionSequence = fixture
                .CreateMany<RetryException>(2, 10)
                .ToArray<Exception>();

            _loggerMock = new FakeLoggerFactory().ResolveLoggerMock<MessageHandlerWithExecutionTracking>();
            _projectionName = fixture.Create<ConnectedProjectionName>();
            _handlerWithoutPolicy = new MessageHandlerWithExecutionTracking(
               _projectionName,
               _loggerMock.AsLogger(),
               exceptionSequence);

            _numberOfRetries = exceptionSequence.Length - 1;
            _numberOfExpectedAttempts = Times.Exactly(1 + _numberOfRetries);
            _initialWait = TimeSpan.FromMilliseconds(fixture.CreatePositive<int>());
            _messages = fixture.CreateMany<StreamMessage>().ToList();

            _sut = new LinearBackOff<RetryException>(_numberOfRetries, _initialWait)
                .ApplyOn(_handlerWithoutPolicy);
        }

        private async Task Act() => await _sut.HandleAsync(_messages, Mock.Of<IStreamGapStrategy>(), CancellationToken.None);

        [Fact]
        public void ThenTheExceptionToRetryIsNotCaught()
        {
            ((Func<Task>)Act)
                .Should()
                .Throw<RetryException>();
        }

        [Fact]
        public async Task ThenHandleActionCallsTheHandlerWithoutAPolicyForTheNumberOfRetriesPlusTheInitialAttempt()
        {
            try
            {
                await Act();
            }
            catch (RetryException)
            { }

            _handlerWithoutPolicy.VerifyExecuted(_messages, _numberOfExpectedAttempts);
        }

        [Fact]
        public async Task ThenAWarningIsLoggedForEachRetry()
        {
            try
            {
                await Act();
            }
            catch (RetryException)
            { }

            for (var i = 1; i <= _numberOfRetries; i++)
            {
                _loggerMock.Verify(
                    LogLevel.Warning,
                    $"Projection '{_projectionName}' failed. Retry attempt #{i} in {_initialWait.Multiply(i).TotalSeconds} seconds.",
                    Times.Once);
            }
        }
    }

    public class WhenMessageHandlerThrowsExceptionsDefinedByRetryPolicy
    {
        private readonly MessageHandlerWithExecutionTracking _handlerWithoutPolicy;
        private readonly IReadOnlyCollection<StreamMessage> _messages;
        private readonly int _numberOfRetries;
        private readonly FakeLogger _loggerMock;
        private readonly ConnectedProjectionName _projectionName;
        private readonly TimeSpan _initialWait;
        private readonly Times _numberOfExpectedAttempts;

        public WhenMessageHandlerThrowsExceptionsDefinedByRetryPolicy()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionNames();

            var exceptionSequence = fixture
                .CreateMany<RetryException>(2, 10)
                .ToArray<Exception>();

            _loggerMock = new FakeLoggerFactory().ResolveLoggerMock<MessageHandlerWithExecutionTracking>();
            _projectionName = fixture.Create<ConnectedProjectionName>();
            _handlerWithoutPolicy = new MessageHandlerWithExecutionTracking(
               _projectionName,
               _loggerMock.AsLogger(),
               exceptionSequence);

            _numberOfRetries = exceptionSequence.Length;
            _numberOfExpectedAttempts = Times.Exactly(1 + _numberOfRetries);
            _initialWait = TimeSpan.FromMilliseconds(fixture.CreatePositive<int>());
            _messages = fixture.CreateMany<StreamMessage>().ToList();

            new LinearBackOff<RetryException>(_numberOfRetries, _initialWait)
                .ApplyOn(_handlerWithoutPolicy)
                .HandleAsync(_messages, Mock.Of<IStreamGapStrategy>(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        [Fact]
        public void ThenHandleActionCallsTheHandlerWithoutAPolicyForTheNumberOfRetriesPlusTheInitialAttempt()
        {
            _handlerWithoutPolicy.VerifyExecuted(_messages, _numberOfExpectedAttempts);
        }

        [Fact]
        public void ThenAWarningIsLoggedForEachRetry()
        {
            for (var i = 1; i <= _numberOfRetries; i++)
            {
                _loggerMock.Verify(
                    LogLevel.Warning,
                    $"Projection '{_projectionName}' failed. Retry attempt #{i} in {_initialWait.Multiply(i).TotalSeconds} seconds.",
                    Times.Once);
            }
        }
    }
}
