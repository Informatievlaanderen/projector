namespace Be.Vlaanderen.Basisregisters.Projector.TestProjections.OtherProjections
{
    using Microsoft.EntityFrameworkCore;
    using ProjectionHandling.Runner;

    public class OtherProjectionContext : RunnerDbContext<OtherProjectionContext>
    {
        public override string ProjectionStateSchema => Schemas.OtherProjections;

        public OtherProjectionContext(DbContextOptions<OtherProjectionContext> options)
            : base(options)
        { }
    }
}
