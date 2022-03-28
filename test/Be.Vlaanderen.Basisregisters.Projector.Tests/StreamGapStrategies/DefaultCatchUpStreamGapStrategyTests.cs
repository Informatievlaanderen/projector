namespace Be.Vlaanderen.Basisregisters.Projector.Tests.StreamGapStrategies
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using ConnectedProjections;
    using FluentAssertions;
    using Infrastructure;
    using Infrastructure.Extensions;
    using Internal;
    using Internal.Configuration;
    using Internal.Exceptions;
    using Internal.StreamGapStrategies;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NodaTime;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;

    public class When_handling_a_message_with_position_and_creation_outside_stream_end_buffers_using_the_default_catchup_stream_gap_strategy
    {
        private readonly ConnectedProjectionIdentifier _projection;
        private readonly IEnumerable<long> _missingPositions;
        private readonly FakeLogger _loggerMock;
        private string _processMessageFunctionStatus;

        public When_handling_a_message_with_position_and_creation_outside_stream_end_buffers_using_the_default_catchup_stream_gap_strategy()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionIdentifiers();

            _projection = fixture.Create<ConnectedProjectionIdentifier>();
            _missingPositions = fixture.CreateMany<long>(1, 10);

            var settings = new StreamGapStrategyConfigurationSettings
            {
                StreamBufferInSeconds = fixture.CreatePositive<int>(),
                PositionBufferSize = fixture.CreatePositive<int>()
            };

            var streamHeadPosition = fixture
                .CreatePositive<long>()
                .WithMinimumValueOf(settings.PositionBufferSize + 100);
            var streamStoreMock = new Mock<IReadonlyStreamStore>();
            streamStoreMock
                .Setup(store => store.ReadHeadPosition(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(streamHeadPosition));

            var now = fixture
                .Create<DateTime>()
                .ToUniversalTime();
            var clockMock = new Mock<IClock>();
            clockMock
                .Setup(clock => clock.GetCurrentInstant())
                .Returns(Instant.FromDateTimeUtc(now));

            var message = (StreamMessage)fixture
                .Build<ConfigurableStreamMessage>()
                .WithPosition(fixture
                    .CreatePositive<long>()
                    .WithMaximumValueOf(streamHeadPosition - settings.PositionBufferSize))
                .WithCreatedUtc(now.AddSeconds(0 - settings.StreamBufferInSeconds))
                .Create();

            var stateMock = new Mock<IProcessedStreamState>();
            stateMock
                .Setup(state => state.DetermineGapPositions(message))
                .Returns(_missingPositions);

            var fakeLoggerFactory = new FakeLoggerFactory();
            _loggerMock = fakeLoggerFactory.ResolveLoggerMock<DefaultCatchUpStreamGapStrategy>();

            _processMessageFunctionStatus = "NotExecuted";

            new DefaultCatchUpStreamGapStrategy(fakeLoggerFactory, settings, streamStoreMock.Object, clockMock.Object)
                .HandleMessage(
                    message,
                    stateMock.Object,
                    (_, token) =>
                    {
                        _processMessageFunctionStatus = "Executed";
                        return Task.CompletedTask;
                    },
                    _projection,
                    fixture.Create<CancellationToken>())
                .GetAwaiter();
        }

        [Fact]
        public void Then_a_missing_stream_messages_warning_is_logged()
        {
            _loggerMock.Verify(
                LogLevel.Warning,
                $"Expected messages at positions [{string.Join(", ", _missingPositions)}] were not processed for {_projection}.",
                Times.Once);
        }

        [Fact]
        public void Then_process_message_should_be_executed()
        {
            _processMessageFunctionStatus
                .Should()
                .Be("Executed");
        }
    }

    public class When_handling_a_message_with_position_and_creation_within_stream_end_buffers_using_the_default_catchup_stream_gap_strategy
    {
        private readonly ConnectedProjectionIdentifier _projection;
        private readonly IEnumerable<long> _missingPositions;
        private readonly FakeLogger _loggerMock;
        private readonly Func<Task> _handlingMessage;
        private string _processMessageFunctionStatus;

        public When_handling_a_message_with_position_and_creation_within_stream_end_buffers_using_the_default_catchup_stream_gap_strategy()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionIdentifiers();

            _projection = fixture.Create<ConnectedProjectionIdentifier>();
            _missingPositions = fixture.CreateMany<long>(1, 10);

            var settings = new StreamGapStrategyConfigurationSettings
            {
                StreamBufferInSeconds = fixture.CreatePositive<int>(),
                PositionBufferSize = fixture.CreatePositive<int>()
            };

            var streamHeadPosition = fixture
                .CreatePositive<long>()
                .WithMinimumValueOf(settings.PositionBufferSize + 100);
            var streamStoreMock = new Mock<IReadonlyStreamStore>();
            streamStoreMock
                .Setup(store => store.ReadHeadPosition(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(streamHeadPosition));

            var now = fixture
                .Create<DateTime>()
                .ToUniversalTime();
            var clockMock = new Mock<IClock>();
            clockMock
                .Setup(clock => clock.GetCurrentInstant())
                .Returns(Instant.FromDateTimeUtc(now));

            var message = (StreamMessage)fixture
                .Build<ConfigurableStreamMessage>()
                .WithPosition(streamHeadPosition
                    .CreateRandomLowerValue()
                    .WithMinimumValueOf(streamHeadPosition + 1 - settings.PositionBufferSize))
                .WithCreatedUtc(now.AddSeconds(1 - settings.StreamBufferInSeconds))
                .Create();

            var stateMock = new Mock<IProcessedStreamState>();
            stateMock
                .Setup(state => state.DetermineGapPositions(message))
                .Returns(_missingPositions);

            var fakeLoggerFactory = new FakeLoggerFactory();
            _loggerMock = fakeLoggerFactory.ResolveLoggerMock<DefaultCatchUpStreamGapStrategy>();

            _processMessageFunctionStatus = "NotExecuted";

            _handlingMessage = async () => await new DefaultCatchUpStreamGapStrategy(fakeLoggerFactory, settings, streamStoreMock.Object, clockMock.Object)
                .HandleMessage(
                    message,
                    stateMock.Object,
                    (_, token) =>
                    {
                        _processMessageFunctionStatus = "Executed";
                        return Task.CompletedTask;
                    },
                    _projection,
                    fixture.Create<CancellationToken>());
        }

        [Fact]
        public async Task Then_a_detected_stream_gap_exception_is_thrown()
        {
            var exception = await _handlingMessage
                .Should()
                .ThrowAsync<StreamGapDetectedException>();

            exception.And.Message
                .Should()
                .Contain(_projection.ToString())
                .And
                .Contain($"[{string.Join(',', _missingPositions)}]");
        }

        [Fact]
        public async Task Then_no_missing_stream_messages_warning_is_logged()
        {
            try
            {
                await _handlingMessage();
            }
            catch { }
            finally
            {
                _loggerMock.Verify(
                    LogLevel.Warning,
                    $"Expected messages at positions [{string.Join(", ", _missingPositions)}] were not processed for {_projection}.",
                    Times.Never);
            }
        }

        [Fact]
        public async Task Then_process_message_should_not_be_executed()
        {
            try
            {
                await _handlingMessage();
            }
            catch { }
            finally
            {
                _processMessageFunctionStatus
                    .Should()
                    .Be("NotExecuted");
            }
        }
    }

    public class When_handling_a_message_with_position_outside_and_creation_within_stream_end_buffer_using_the_default_catchup_stream_gap_strategy
    {
        private readonly ConnectedProjectionIdentifier _projection;
        private readonly IEnumerable<long> _missingPositions;
        private readonly FakeLogger _loggerMock;
        private string _processMessageFunctionStatus;

        public When_handling_a_message_with_position_outside_and_creation_within_stream_end_buffer_using_the_default_catchup_stream_gap_strategy()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionIdentifiers();

            _projection = fixture.Create<ConnectedProjectionIdentifier>();
            _missingPositions = fixture.CreateMany<long>(1, 10);

            var settings = new StreamGapStrategyConfigurationSettings
            {
                StreamBufferInSeconds = fixture.CreatePositive<int>(),
                PositionBufferSize = fixture.CreatePositive<int>()
            };

            var streamHeadPosition = fixture
                .CreatePositive<long>()
                .WithMinimumValueOf(settings.PositionBufferSize + 100);
            var streamStoreMock = new Mock<IReadonlyStreamStore>();
            streamStoreMock
                .Setup(store => store.ReadHeadPosition(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(streamHeadPosition));

            var now = fixture
                .Create<DateTime>()
                .ToUniversalTime();
            var clockMock = new Mock<IClock>();
            clockMock
                .Setup(clock => clock.GetCurrentInstant())
                .Returns(Instant.FromDateTimeUtc(now));

            var message = (StreamMessage)fixture
                .Build<ConfigurableStreamMessage>()
                .WithPosition(streamHeadPosition
                    .CreateRandomLowerValue()
                    .WithMaximumValueOf(streamHeadPosition - settings.PositionBufferSize))
                .WithCreatedUtc(now.AddSeconds(1 - settings.StreamBufferInSeconds))
                .Create();

            var stateMock = new Mock<IProcessedStreamState>();
            stateMock
                .Setup(state => state.DetermineGapPositions(message))
                .Returns(_missingPositions);

            var fakeLoggerFactory = new FakeLoggerFactory();
            _loggerMock = fakeLoggerFactory.ResolveLoggerMock<DefaultCatchUpStreamGapStrategy>();

            _processMessageFunctionStatus = "NotExecuted";

            new DefaultCatchUpStreamGapStrategy(fakeLoggerFactory, settings, streamStoreMock.Object, clockMock.Object)
                .HandleMessage(
                    message,
                    stateMock.Object,
                    (_, token) =>
                    {
                        _processMessageFunctionStatus = "Executed";
                        return Task.CompletedTask;
                    },
                    _projection,
                    fixture.Create<CancellationToken>())
                .GetAwaiter();
        }
        
        [Fact]
        public void Then_a_missing_stream_messages_warning_is_logged()
        {
            _loggerMock.Verify(
                LogLevel.Warning,
                $"Expected messages at positions [{string.Join(", ", _missingPositions)}] were not processed for {_projection}.",
                Times.Once);
        }

        [Fact]
        public void Then_process_message_should_be_executed()
        {
            _processMessageFunctionStatus
                .Should()
                .Be("Executed");
        }
    }

    public class When_handling_a_message_with_position_within_and_creation_outside_stream_end_buffer_using_the_default_catchup_stream_gap_strategy
    {
        private readonly ConnectedProjectionIdentifier _projection;
        private readonly IEnumerable<long> _missingPositions;
        private readonly FakeLogger _loggerMock;
        private string _processMessageFunctionStatus;

        public When_handling_a_message_with_position_within_and_creation_outside_stream_end_buffer_using_the_default_catchup_stream_gap_strategy()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionIdentifiers();

            _projection = fixture.Create<ConnectedProjectionIdentifier>();
            _missingPositions = fixture.CreateMany<long>(1, 10);

            var settings = new StreamGapStrategyConfigurationSettings
            {
                StreamBufferInSeconds = fixture.CreatePositive<int>(),
                PositionBufferSize = fixture.CreatePositive<int>()
            };

            var streamHeadPosition = fixture
                .CreatePositive<long>()
                .WithMinimumValueOf(settings.PositionBufferSize + 100);
            var streamStoreMock = new Mock<IReadonlyStreamStore>();
            streamStoreMock
                .Setup(store => store.ReadHeadPosition(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(streamHeadPosition));

            var now = fixture
                .Create<DateTime>()
                .ToUniversalTime();
            var clockMock = new Mock<IClock>();
            clockMock
                .Setup(clock => clock.GetCurrentInstant())
                .Returns(Instant.FromDateTimeUtc(now));

            var message = (StreamMessage)fixture
                .Build<ConfigurableStreamMessage>()
                .WithPosition(streamHeadPosition
                    .CreateRandomLowerValue()
                    .WithMinimumValueOf(streamHeadPosition + 1 - settings.PositionBufferSize))
                .WithCreatedUtc(now.AddSeconds(0 - settings.StreamBufferInSeconds))
                .Create();

            var stateMock = new Mock<IProcessedStreamState>();
            stateMock
                .Setup(state => state.DetermineGapPositions(message))
                .Returns(_missingPositions);

            var fakeLoggerFactory = new FakeLoggerFactory();
            _loggerMock = fakeLoggerFactory.ResolveLoggerMock<DefaultCatchUpStreamGapStrategy>();

            _processMessageFunctionStatus = "NotExecuted";

            new DefaultCatchUpStreamGapStrategy(fakeLoggerFactory, settings, streamStoreMock.Object, clockMock.Object)
                .HandleMessage(
                    message,
                    stateMock.Object,
                    (_, token) =>
                    {
                        _processMessageFunctionStatus = "Executed";
                        return Task.CompletedTask;
                    },
                    _projection,
                    fixture.Create<CancellationToken>())
                .GetAwaiter();
        }
        
        [Fact]
        public void Then_a_missing_stream_messages_warning_is_logged()
        {
            _loggerMock.Verify(
                LogLevel.Warning,
                $"Expected messages at positions [{string.Join(", ", _missingPositions)}] were not processed for {_projection}.",
                Times.Once);
        }

        [Fact]
        public void Then_process_message_should_be_executed()
        {
            _processMessageFunctionStatus
                .Should()
                .Be("Executed");
        }

    }
}
