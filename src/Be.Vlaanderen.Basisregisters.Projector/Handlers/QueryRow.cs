namespace Be.Vlaanderen.Basisregisters.Projector.Handlers
{
    public class QueryRow
    {
        public object? ExternalId { get; set; }
        public object? InternalId { get; set; }
        public object? EventId { get; set; }
        public string? ChangeType { get; set; }
    }
}
