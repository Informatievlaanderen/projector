namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using Newtonsoft.Json;

    [JsonConverter(typeof(ConnectedProjectionIdentifierJsonConverter))]
    public class ConnectedProjectionIdentifier
    {
        private readonly string _identifier;

        internal ConnectedProjectionIdentifier(Type connectedProjectionType)
            : this(connectedProjectionType.FullName ?? string.Empty) {}

        internal ConnectedProjectionIdentifier(string identifier)
            => _identifier = !string.IsNullOrWhiteSpace(identifier) ? identifier : throw new ArgumentNullException(nameof(identifier));

        public override bool Equals(object? obj)
        {
            return obj switch
            {
                ConnectedProjectionIdentifier identifier => Equals(identifier),
                string identifier => Equals(identifier),
                _ => false
            };
        }

        public bool Equals(ConnectedProjectionIdentifier? other) => other != null && Equals(other._identifier);

        public bool Equals(string other) => string.Equals(_identifier, other, StringComparison.InvariantCultureIgnoreCase);

        public override int GetHashCode() => _identifier?.ToLowerInvariant().GetHashCode() ?? 0;

        public override string ToString() => _identifier;

        public static bool operator ==(ConnectedProjectionIdentifier? left, ConnectedProjectionIdentifier? right) => Equals(left, right);

        public static bool operator !=(ConnectedProjectionIdentifier? left, ConnectedProjectionIdentifier? right) => !Equals(left, right);

        public static implicit operator string(ConnectedProjectionIdentifier projectionIdentifier) => projectionIdentifier.ToString() ?? string.Empty;
    }

    public class ConnectedProjectionIdentifierJsonConverter : JsonConverter<ConnectedProjectionIdentifier>
    {
        public override bool CanRead => false;

        public override void WriteJson(
            JsonWriter writer,
            ConnectedProjectionIdentifier? value,
            JsonSerializer serializer)
            => writer.WriteValue(value?.ToString());

        public override ConnectedProjectionIdentifier ReadJson(
            JsonReader reader,
            Type objectType,
            ConnectedProjectionIdentifier? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
            => throw new NotImplementedException();
    }
}
