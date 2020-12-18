namespace Be.Vlaanderen.Basisregisters.Projector.TestProjections.Projections
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ProcessedEvent
    {
        public Guid? Id { get; set; }
        public string Event { get; set; }
        public DateTime EvenTime { get; set; }
        public long Position { get; set; }
    }

    public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
    {
        private const string TableName = "ProcessedEvents";

        public void Configure(EntityTypeBuilder<ProcessedEvent> b)
        {
            b.ToTable(TableName, Schemas.Projections)
                .HasKey(x => x.Id)
                .IsClustered(false);

            b.Property(x => x.Event);
            b.Property(x => x.Position);
        }
    }
}
