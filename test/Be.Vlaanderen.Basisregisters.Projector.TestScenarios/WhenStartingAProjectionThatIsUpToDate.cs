namespace Be.Vlaanderen.Basisregisters.Projector.TestScenarios
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using ConnectedProjections;
    using FluentAssertions;
    using Infrastructure;
    using Internal;
    using SqlStreamStore;
    using TestProjections.Messages;
    using TestProjections.Projections;
    using Xunit;

    public class WhenStartingAProjectionThatIsUpToDate : Scenario
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
            await ProjectionManager.Start(CancellationToken.None);
            _waitForProjection.WaitOne();
            _waitForProjection.Reset();
            await ProjectionManager.Stop(CancellationToken.None);
            await Task.Delay(500);
        }

        private void MessageWasHandled() => _waitForProjection.Set();

        [Fact]
        public async Task VerifySetup()
        {
            (await Resolve<IReadonlyStreamStore>().ReadHeadPosition())
                .Should()
                .BeGreaterThan(HeadPosition.NoMessages);

            (await Resolve<IConnectedProjectionContext<ProjectionContext>>().GetProjectionPosition(_projection, CancellationToken.None))
                .Should()
                .Be(0);

            ProjectionManager
                .GetRegisteredProjections()
                .Should()
                .Contain(connectedProjection =>
                    connectedProjection.Id == _projection
                    && connectedProjection.State == ConnectedProjectionState.Stopped);
        }

        [Fact]
        public async Task Then_the_projection_is_subscribed()
        {
            await ProjectionManager.Start(_projection, CancellationToken.None);

            await Task.Delay(250);
            ProjectionManager
                .GetRegisteredProjections()
                .Should()
                .Contain(connectedProjection =>
                    connectedProjection.Id == _projection
                    && connectedProjection.State == ConnectedProjectionState.Subscribed);
        }

        [Fact]
        public async Task Then_the_projection_process_new_events()
        {
            await ProjectionManager.Start(_projection, CancellationToken.None);
            await Task.Delay(250);
            var somethingHappened = Fixture.Create<SomethingHappened>();
            await PushToStream(somethingHappened);

            _waitForProjection.WaitOne();
            await Task.Delay(100);
            var assertionContext = new ProjectionContext(CreateContextOptionsFor<ProjectionContext>());
            assertionContext.ProcessedEvents
                .Should().Contain(@event =>
                    @event.Position == PushedMessages.Count - 1
                    && @event.Event == somethingHappened.GetType().Name
                    && @event.EvenTime == somethingHappened.On);
        }
    }
}
