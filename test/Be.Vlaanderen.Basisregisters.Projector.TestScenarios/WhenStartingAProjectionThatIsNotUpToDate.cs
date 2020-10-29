namespace Be.Vlaanderen.Basisregisters.Projector.TestScenarios
{
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

    public class WhenStartingAProjectionThatIsNotUpToDate : Scenario
    {
        private ConnectedProjectionName _projection;
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
            _projection = new ConnectedProjectionName(typeof(TrackHandledEventsProjection));
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
                    connectedProjection.Name == _projection
                    && connectedProjection.State == ConnectedProjectionState.Stopped);
        }

        [Fact]
        public async Task Then_the_projection_is_catching_up()
        {
            await ProjectionManager.Start(_projection, CancellationToken.None);

            _waitForProjection.WaitOne();
            ProjectionManager
                .GetRegisteredProjections()
                .Should()
                .Contain(connectedProjection =>
                    connectedProjection.Name == _projection
                    && connectedProjection.State == ConnectedProjectionState.CatchingUp);
        }

        [Fact]
        public async Task Then_the_projection_is_subscribed_once_caught_up()
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
                    connectedProjection.Name == _projection
                    && connectedProjection.State == ConnectedProjectionState.Subscribed);
            var assertionContext = new ProjectionContext(CreateContextOptionsFor<ProjectionContext>());
            assertionContext.ProcessedEvents
                .Should()
                .ContainAll(
                    PushedMessages,
                    (processedMessage, message, i) => processedMessage.Position == i && processedMessage.Event == message.GetType().Name && processedMessage.EvenTime == message.On);

        }

        [Fact]
        public async Task Then_the_projection_processed_the_next_events_as_subscription()
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
                    connectedProjection.Name == _projection
                    && connectedProjection.State == ConnectedProjectionState.Subscribed);

            await PushToStream(Fixture.CreateMany<SomethingHappened>(4));
            await Task.Delay(500);

            var assertionContext = new ProjectionContext(CreateContextOptionsFor<ProjectionContext>());
            assertionContext.ProcessedEvents
                .Should()
                .ContainAll(
                    PushedMessages,
                    (processedMessage, message, i) => processedMessage.Position == i && processedMessage.Event == message.GetType().Name && processedMessage.EvenTime == message.On);

        }
    }
}
