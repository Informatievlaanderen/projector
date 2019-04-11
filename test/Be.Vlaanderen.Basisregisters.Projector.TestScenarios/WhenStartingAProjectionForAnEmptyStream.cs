namespace Be.Vlaanderen.Basisregisters.Projector.TestScenarios
{
    using System.Threading.Tasks;
    using Autofac;
    using ConnectedProjections;
    using FluentAssertions;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using TestProjections.Projections;
    using Xunit;

    public class WhenStartingAProjectionForAnEmptyStream : Scenario
    {
        private ConnectedProjectionName _projection;

        protected override void ContainerSetup(ContainerBuilder builder)
        {
            builder
                .RegisterProjections<TrackHandledEventsProjection, ProjectionContext>()
                .RegisterProjections<SlowProjections, ProjectionContext>();
        }

        protected override void Setup()
        {
            _projection = new ConnectedProjectionName(typeof(TrackHandledEventsProjection));
        }

        [Fact]
        public async Task VerifySetup()
        {
            (await Resolve<IStreamStore>().ReadHeadPosition())
                .Should()
                .Be(ExpectedVersion.NoStream);

            ProjectionManager
                .GetRegisteredProjections()
                .Should()
                .Contain(connectedProjection =>
                    connectedProjection.Name.Equals(_projection)
                    && connectedProjection.State == ConnectedProjectionState.Stopped);
        }

        [Fact]
        public async Task Then_the_projection_is_subscribed()
        {
            ProjectionManager.Start(_projection);

            await Task.Delay(1000);
            ProjectionManager
                .GetRegisteredProjections()
                .Should()
                .Contain(connectedProjection =>
                    connectedProjection.Name.Equals(_projection)
                    && connectedProjection.State == ConnectedProjectionState.Subscribed);
        }
    }
}