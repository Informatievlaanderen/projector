namespace Be.Vlaanderen.Basisregisters.Projector.TestProjections.Projections
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using ProjectionHandling.Runner;

    public class ProjectionContext : RunnerDbContext<ProjectionContext>
    {
        public override string ProjectionStateSchema => Schemas.Projections;

        public DbSet<ProcessedEvent> ProcessedEvents { get; set; }

        public ProjectionContext(DbContextOptions<ProjectionContext> options)
            : base(options)
        { }

        public ProjectionContext()
            : this(new DbContextOptionsBuilder<ProjectionContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options)
        { }
    }
}
