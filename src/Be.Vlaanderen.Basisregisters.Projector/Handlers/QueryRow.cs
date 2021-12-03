namespace Be.Vlaanderen.Basisregisters.Projector.Handlers
{
    public class QueryRow
    {
        public object? ExternalId { get; set; }
        public object? InternalId { get; set; }
        public object? EventId { get; set; }
        public string? ChangeType { get; set; }
        public string? EventData { get; set; }
        public string? Timestamp { get; set; }

        public override string ToString()
        {
            return $"{nameof(ExternalId)}: {ExternalId}, {nameof(InternalId)}: {InternalId}, {nameof(EventId)}: {EventId}, {nameof(ChangeType)}: {ChangeType}, {nameof(EventData)}: {EventData}, {nameof(Timestamp)}: {Timestamp}";
        }
    }
}
