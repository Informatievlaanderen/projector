namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Runners
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
    using Internal.Commands;
    using Internal.Commands.Subscription;
    using Internal.Configuration;
    using Internal.Exceptions;
    using Internal.Runners;
    using Internal.StreamGapStrategies;
    using Microsoft.Extensions.Logging;
    using Moq;
    using SqlStreamStore.Streams;
    using Xunit;

    public class When_the_subscription_runner_processing_a_stream_event_throws_a_detected_stream_gap_exception
    {
        private const string MissingMessageProjectionIdentifier = "throws-missing-messages";
        private readonly StreamStoreConnectedProjectionsSubscriptionRunner _sut;
        private readonly Mock<IConnectedProjectionsCommandBus> _commandBusMock;
        private readonly List<IConnectedProjection> _registeredProjections;
        private readonly FakeLogger _loggerMock;
        private readonly StreamGapStrategyConfigurationSettings _gapStrategySettings;

        public When_the_subscription_runner_processing_a_stream_event_throws_a_detected_stream_gap_exception()
        {
            var fixture = new Fixture();

            _commandBusMock = new Mock<IConnectedProjectionsCommandBus>();
            var streamMock = new Mock<IConnectedProjectionsStreamStoreSubscription>();
            streamMock
                .SetupGet(stream => stream.StreamIsRunning)
                .Returns(true);

            var contextMock = new Mock<IConnectedProjectionContext<FakeProjectionContext>>();
            contextMock
                .Setup(context => context.GetProjectionPosition(It.IsAny<ConnectedProjectionIdentifier>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((long?)null);

            var missingMessagesPositions = fixture.CreateMany<long>(2,10).ToList();
            _registeredProjections = new List<IConnectedProjection>
            {
                new FakeProjection(
                    $"{MissingMessageProjectionIdentifier}-1",
                    ThrowMissingMessageException(missingMessagesPositions),
                    contextMock.Object),
                new FakeProjection(
                    "do-nothing",
                    (messages, strategy, name, ct) => Task.CompletedTask,
                    contextMock.Object),
                new FakeProjection(
                    $"{MissingMessageProjectionIdentifier}-2",
                    ThrowMissingMessageException(missingMessagesPositions),
                    contextMock.Object)
            };

            var registeredProjectionsMock = new Mock<IRegisteredProjections>();
            registeredProjectionsMock
                .Setup(projections => projections.GetProjection(It.IsAny<ConnectedProjectionIdentifier>()))
                .Returns((ConnectedProjectionIdentifier projectionId) => _registeredProjections.FirstOrDefault(projection => projection.Id.Equals(projectionId)));

            var loggerFactory = new FakeLoggerFactory();
            _loggerMock = loggerFactory.ResolveLoggerMock<StreamStoreConnectedProjectionsSubscriptionRunner>();

            _gapStrategySettings = fixture.Create<StreamGapStrategyConfigurationSettings>();
            var gapStrategyMock = new Mock<IStreamGapStrategy>();
            gapStrategyMock
                .SetupGet(strategy => strategy.Settings)
                .Returns(_gapStrategySettings);

            _sut = new StreamStoreConnectedProjectionsSubscriptionRunner(
                registeredProjectionsMock.Object,
                streamMock.Object,
                _commandBusMock.Object,
                gapStrategyMock.Object,
                loggerFactory);

            foreach (var projection in _registeredProjections)
                _sut.HandleSubscriptionCommand(new Subscribe(projection.Id)).GetAwaiter();

            VerifySetup();
            _sut.HandleSubscriptionCommand(new ProcessStreamEvent(fixture.Create<StreamMessage>(), fixture.Create<CancellationToken>())).GetAwaiter();
        }

        private void VerifySetup()
        {
            foreach (var projection in _registeredProjections)
                _sut.HasSubscription(projection.Id)
                    .Should()
                    .BeTrue($"expected {projection.Id} to be subscribed");
        }

        private static Func<IEnumerable<StreamMessage>, IStreamGapStrategy, ConnectedProjectionIdentifier, CancellationToken, Task> ThrowMissingMessageException(IEnumerable<long> missingMessagesPositions)
            => (messages, strategy, name, ct) => throw new ConnectedProjectionMessageHandlingException(new StreamGapDetectedException(missingMessagesPositions, name), name, null);

        private static bool IsMissingMessageProjection(IConnectedProjection projection)
            => projection.Id.ToString().Contains(MissingMessageProjectionIdentifier);

        [Fact]
        public void Then_a_restart_projection_is_queued_for_each_projection_that_threw_the_exception()
        {
            foreach (var projection in _registeredProjections.Where(IsMissingMessageProjection))
                _commandBusMock.Verify(
                    bus => bus.Queue(
                        It.Is<Restart>(restart =>
                            restart.After == TimeSpan.FromSeconds(_gapStrategySettings.RetryDelayInSeconds) &&
                            restart.Projection.Equals(projection.Id))),
                    Times.Once);
        }

        [Fact]
        public void Then_a_restart_warning_is_logged_for_each_projection_that_threw_the_exception()
        {
            foreach (var projection in _registeredProjections.Where(IsMissingMessageProjection))
                _loggerMock.Verify(
                    LogLevel.Warning,
                    $"Detected gap in the message stream for subscribed projection. Unsubscribed projection {projection.Id} and queued restart in {_gapStrategySettings.RetryDelayInSeconds} seconds.",
                    Times.Once);
        }

        [Fact]
        public void Then_the_projections_that_threw_the_exception_are_removed()
        {
            foreach (var projection in _registeredProjections.Where(IsMissingMessageProjection))
                _sut.HasSubscription(projection.Id)
                    .Should()
                    .BeFalse($"expected {projection.Id} to be removed from subscriptions");
        }
    }
}
