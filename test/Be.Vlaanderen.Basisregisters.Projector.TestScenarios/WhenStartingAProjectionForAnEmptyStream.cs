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

    public class WhenStartingAProjectionForAnEmptyStream : Scenario
    {
        private ConnectedProjectionIdentifier _projection;
        private AutoResetEvent _projectionStarted;

        protected override void ContainerSetup(ContainerBuilder builder)
        {
            builder
                .RegisterProjections<TrackHandledEventsProjection, ProjectionContext>(
                    () => new TrackHandledEventsProjection(MessageWasHandled),
                    ConnectedProjectionSettings.Default
                )
                .RegisterProjections<SlowProjections, ProjectionContext>(ConnectedProjectionSettings.Default);
        }

        protected override Task Setup()
        {
            _projection = new ConnectedProjectionIdentifier(typeof(TrackHandledEventsProjection));
            _projectionStarted = new AutoResetEvent(false);
            return Task.CompletedTask;
        }

        private ConnectedProjectionState GetStateFor(ConnectedProjectionIdentifier projection) => ProjectionManager
            .GetRegisteredProjections()
            .Single(connectedProjection => connectedProjection.Id == projection)
            .State;

        private void MessageWasHandled() => _projectionStarted?.Set();

        [Fact]
        public async Task VerifySetup()
        {
            (await Resolve<IReadonlyStreamStore>().ReadHeadPosition())
                .Should()
                .Be(HeadPosition.NoMessages);

            GetStateFor(_projection)
                .Should()
                .Be(ConnectedProjectionState.Stopped);
        }

        [Fact]
        public async Task Then_the_projection_is_subscribed()
        {
            await ProjectionManager.Start(_projection, CancellationToken.None);

            await Task.Delay(1000);

            GetStateFor(_projection)
                .Should()
                .Be(ConnectedProjectionState.Subscribed);
        }

        [Fact]
        public async Task Then_the_events_pushed_to_the_store_while_subscribed_are_handled()
        {
            await ProjectionManager.Start(_projection, CancellationToken.None);
            // wait for projection to be in subscription
            while (GetStateFor(_projection) != ConnectedProjectionState.Subscribed)
                await Task.Delay(25);

            var message = Fixture.Create<SomethingHappened>();
            await PushToStream(message);
            _projectionStarted.WaitOne(1000);
            await Task.Delay(500);

            var assertContext = new ProjectionContext(CreateContextOptionsFor<ProjectionContext>());
            assertContext.ProcessedEvents
                .Should()
                .Contain(@event => @event.Position == 0L && @event.Event == message.GetType().Name && @event.EventTime == message.On);
        }
    }
}
