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

    public class WhenStartingAllProjectionForAnEmptyStream : Scenario
    {
        protected override void ContainerSetup(ContainerBuilder builder)
        {
            builder
                .RegisterProjections<TrackHandledEventsProjection, ProjectionContext>()
                .RegisterProjections<SlowProjections, ProjectionContext>()
                .RegisterProjections<FastProjections, ProjectionContext>();
        }

        protected override void Setup()
        { }

        [Fact]
        public async Task VerifySetup()
        {
            (await Resolve<IStreamStore>().ReadHeadPosition())
                .Should()
                .Be(ExpectedVersion.NoStream);

            ProjectionManager
                .GetRegisteredProjections()
                .Should()
                .OnlyContain(connectedProjection => connectedProjection.State == ConnectedProjectionState.Stopped);
        }

        [Fact]
        public async Task Then_all_projections_are_subscribed()
        {
            ProjectionManager.Start();

            await Task.Delay(1000);
            ProjectionManager
                .GetRegisteredProjections()
                .Should()
                .OnlyContain(connectedProjection => connectedProjection.State == ConnectedProjectionState.Subscribed);
        }
    }
}
