namespace Be.Vlaanderen.Basisregisters.Projector.TestProjections.Projections
{
    using ProjectionHandling.Runner;

    public class ProjectionContext : RunnerDbContext<ProjectionContext>
    {
        public override string ProjectionStateSchema => "TestProjectionsSchema";
    }
}
