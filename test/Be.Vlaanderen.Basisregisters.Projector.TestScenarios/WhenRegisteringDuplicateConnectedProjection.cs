namespace Be.Vlaanderen.Basisregisters.Projector.TestScenarios
{
    using System.Linq;
    using Autofac;
    using ConnectedProjections;
    using FluentAssertions;
    using TestProjections.Projections;
    using Xunit;

    public class WhenRegisteringDuplicateConnectedProjection : Scenario
    {
        protected override void ContainerSetup(ContainerBuilder builder)
        {
            builder
                .RegisterStreamStoreProjections<TrackHandledEventsProjection, ProjectionContext>(StreamStoreConnectedProjectionSettings.Default)
                .RegisterStreamStoreProjections<SlowProjections, ProjectionContext>(StreamStoreConnectedProjectionSettings.Default)
                .RegisterStreamStoreProjections<TrackHandledEventsProjection, ProjectionContext>(StreamStoreConnectedProjectionSettings.Default);
        }

        [Fact]
        public void Then_only_2_are_registered()
        {
            ProjectionManager.GetRegisteredProjections().Count().Should().Be(2);
        }
    }
}
