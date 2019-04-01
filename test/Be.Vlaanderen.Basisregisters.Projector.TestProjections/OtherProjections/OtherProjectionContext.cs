namespace Be.Vlaanderen.Basisregisters.Projector.TestProjections.OtherProjections
{
    using ProjectionHandling.Runner;

    public class OtherProjectionContext : RunnerDbContext<OtherProjectionContext>
    {
        public override string ProjectionStateSchema => "OtherTestProjections";
    }
}
