namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Runners
{
    using System;
    using System.Linq;
    using System.Threading;
    using AutoFixture;
    using ConnectedProjections;
    using Infrastructure;
    using Infrastructure.Extensions;
    using Internal;
    using Internal.Commands;
    using Internal.Commands.CatchUp;
    using Internal.Configuration;
    using Internal.Exceptions;
    using Internal.StreamGapStrategies;
    using Microsoft.Extensions.Logging;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;

    public class When_a_projection_catch_up_processes_a_stream_and_throws_a_detected_stream_gap_exception
    {
        private readonly IConnectedProjection<FakeProjectionContext> _projection;
        private readonly Mock<IConnectedProjectionsCommandBus> _commandBusMock;
        private readonly StreamGapStrategyConfigurationSettings _gapStrategySettings;
        private readonly FakeLogger _loggerMock;

        public When_a_projection_catch_up_processes_a_stream_and_throws_a_detected_stream_gap_exception()
        {
            var fixture = new Fixture()
                .CustomizeConnectedProjectionNames();

            var contextMock = new Mock<IConnectedProjectionContext<FakeProjectionContext>>();
            contextMock
                .Setup(context => context.GetProjectionPosition(It.IsAny<ConnectedProjectionName>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((long?)null);

            var streamMock = new Mock<IReadonlyStreamStore>();
            streamMock
                .Setup(store =>
                    store.ReadAllForwards(
                        It.IsAny<long>(),
                        It.IsAny<int>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                    {
                        var position = fixture.CreatePositive<long>().WithMaximumValueOf(long.MaxValue - 1000);
                        return new ReadAllPage(
                            position,
                            position.CreateRandomHigherValue(),
                            true,
                            ReadDirection.Forward,
                            (p, token) => throw new NotImplementedException(),
                            fixture.CreateMany<StreamMessage>(2, 10).ToArray());
                    });

            _commandBusMock = new Mock<IConnectedProjectionsCommandBus>();

            _gapStrategySettings = fixture.Create<StreamGapStrategyConfigurationSettings>();
            var gapStrategyMock = new Mock<IStreamGapStrategy>();
            gapStrategyMock
                .SetupGet(strategy => strategy.Settings)
                .Returns(_gapStrategySettings);

            _loggerMock = new FakeLogger();
            _projection = new FakeProjection(
                "catch-up-dummy",
                (messages, strategy, name, ct)
                    => throw new ConnectedProjectionMessageHandlingException(
                        new StreamGapDetectedException(fixture.CreateMany<long>(1, 10), name),
                        name,
                        new ActiveProcessedStreamState(0)),
                contextMock.Object);

            var sut = new ConnectedProjectionCatchUp<FakeProjectionContext>(
                _projection,
                streamMock.Object,
                _commandBusMock.Object,
                gapStrategyMock.Object,
                _loggerMock.AsLogger());

            sut.CatchUpAsync(CancellationToken.None).GetAwaiter();
        }

        [Fact]
        public void Then_a_restart_projection_is_queued()
        {
                _commandBusMock.Verify(
                    bus => bus.Queue(
                        It.Is<Restart>(restart =>
                            restart.After == TimeSpan.FromSeconds(_gapStrategySettings.RetryDelayInSeconds) &&
                            restart.ProjectionName.Equals(_projection.Name))),
                    Times.Once);
        }

        [Fact]
        public void Then_a_remove_catchup_is_queued()
        {
                _commandBusMock.Verify(
                    bus => bus.Queue(
                        It.Is<RemoveStoppedCatchUp>(stopped => stopped.ProjectionName.Equals(_projection.Name))),
                    Times.Once);
        }

        [Fact]
        public void Then_a_restart_warning_is_logged()
        {
            _loggerMock.Verify(
                LogLevel.Warning,
                $"Detected gap in the message stream for catching up projection. Aborted projection {_projection.Name} and queued restart in {_gapStrategySettings.RetryDelayInSeconds} seconds.",
                Times.Once);
        }

        [Fact]
        public void Then_the_catch_up_is_aborted()
        {
            _loggerMock.Verify(
                LogLevel.Warning,
                $"Stopping catch up {_projection.Name}: {CatchUpStopReason.Aborted}",
                Times.Once);
        }
    }
}
