namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using ProjectionHandling.Runner;

    public class ProjectionContext : RunnerDbContext<ProjectionContext>
    {
        public override string ProjectionStateSchema => "TestSchema";
    }
}
