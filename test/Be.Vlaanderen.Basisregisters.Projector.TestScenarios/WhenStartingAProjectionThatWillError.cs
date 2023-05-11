namespace Be.Vlaanderen.Basisregisters.Projector.TestScenarios
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using ConnectedProjections;
    using FluentAssertions;
    using Infrastructure;
    using SqlStreamStore;
    using TestProjections.Messages;
    using TestProjections.Projections;
    using Xunit;

    public class WhenStartingAProjectionThatWillError : Scenario
    {
        private ConnectedProjectionIdentifier _projection;
        private AutoResetEvent _waitForProjection;

        protected override void ContainerSetup(ContainerBuilder builder)
        {
            builder.RegisterProjections<TrackHandledEventsProjection, ProjectionContext>(
                () => new TrackHandledEventsProjection(MessageWasHandled),
                ConnectedProjectionSettings.Default
            );
        }

        protected override async Task Setup()
        {
            _waitForProjection = new AutoResetEvent(false);
            _projection = new ConnectedProjectionIdentifier(typeof(TrackHandledEventsProjection));
            await PushToStream(Fixture.Create<SomethingHappened>());
            await PushToStream(Fixture.Create<DelayWasScheduled>());
        }

        private void MessageWasHandled() => _waitForProjection.Set();

        [Fact]
        public async Task VerifySetup()
        {
            (await Resolve<IReadonlyStreamStore>().ReadHeadPosition())
                .Should()
                .BeGreaterThan(HeadPosition.NoMessages);

            ProjectionManager
                .GetRegisteredProjections()
                .Should()
                .Contain(connectedProjection =>
                    connectedProjection.Id == _projection
                    && connectedProjection.State == ConnectedProjectionState.Stopped);
        }

        [Fact]
        public async Task Then_the_projection_is_stopped_and_not_caught_up()
        {
            await ProjectionManager.Start(_projection, CancellationToken.None);

            await PushToStream(Fixture.Create<ErrorHappened>());

            _waitForProjection.WaitOne();
            _waitForProjection.Reset();
            _waitForProjection.WaitOne();
            _waitForProjection.Reset();
            _waitForProjection.WaitOne();
            await Task.Delay(1000);

            ProjectionManager
                .GetRegisteredProjections()
                .Should()
                .Contain(connectedProjection =>
                    connectedProjection.Id == _projection
                    && connectedProjection.State == ConnectedProjectionState.Stopped);

            var assertionContext = new ProjectionContext(CreateContextOptionsFor<ProjectionContext>());
            assertionContext.ProcessedEvents.Should().BeEmpty();
        }

        [Fact]
        public async Task Then_the_projection_processed_the_next_events_until_crash_as_subscription()
        {
            await ProjectionManager.Start(_projection, CancellationToken.None);

            _waitForProjection.WaitOne();
            _waitForProjection.Reset();
            _waitForProjection.WaitOne();
            await Task.Delay(1000);

            ProjectionManager
                .GetRegisteredProjections()
                .Should()
                .Contain(connectedProjection =>
                    connectedProjection.Id == _projection
                    && connectedProjection.State == ConnectedProjectionState.Subscribed);

            await PushToStream(Fixture.CreateMany<SomethingHappened>(4));
            await PushToStream(Fixture.Create<ErrorHappened>());
            await Task.Delay(500);

            ProjectionManager
                .GetRegisteredProjections()
                .Should()
                .Contain(connectedProjection =>
                    connectedProjection.Id == _projection
                    && connectedProjection.State == ConnectedProjectionState.Stopped);

            var assertionContext = new ProjectionContext(CreateContextOptionsFor<ProjectionContext>());
            assertionContext.ProcessedEvents.Should().HaveCount(6);
        }
    }
}
