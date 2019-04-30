namespace Be.Vlaanderen.Basisregisters.Projector.TestScenarios
{
    using System.Linq;
    using Autofac;
    using FluentAssertions;
    using TestProjections.Projections;
    using Xunit;

    public class WhenRegisteringDuplicateConnectedProjection : Scenario
    {
        protected override void ContainerSetup(ContainerBuilder builder)
        {
            builder
                .RegisterProjections<TrackHandledEventsProjection, ProjectionContext>()
                .RegisterProjections<SlowProjections, ProjectionContext>()
                .RegisterProjections<TrackHandledEventsProjection, ProjectionContext>();
        }

        [Fact]
        public void Then_only_2_are_registered()
        {
            ProjectionManager.GetRegisteredProjections().Count().Should().Be(2);
        }
    }
}
