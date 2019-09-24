namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using Newtonsoft.Json;

    [JsonConverter(typeof(ConnectedProjectionNameJsonConverter))]
    public class ConnectedProjectionName
    {
        private readonly string _name;

        internal ConnectedProjectionName(Type connectedProjectionType) => _name = connectedProjectionType?.FullName;

        internal ConnectedProjectionName(string name) => _name = name;

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case ConnectedProjectionName name:
                    return Equals(name);

                case string nameString:
                    return Equals(nameString);

                default:
                    return false;
            }
        }

        public bool Equals(ConnectedProjectionName other) => other != null && Equals(other._name);

        public bool Equals(string other) => string.Equals(_name, other, StringComparison.InvariantCultureIgnoreCase);

        public override int GetHashCode() => _name?.ToLowerInvariant().GetHashCode() ?? 0;

        public override string ToString() => _name;

        public static bool operator ==(ConnectedProjectionName left, ConnectedProjectionName right) => Equals(left, right);

        public static bool operator !=(ConnectedProjectionName left, ConnectedProjectionName right) => !Equals(left, right);

        public static implicit operator string(ConnectedProjectionName name) => name?.ToString();
    }

    public class ConnectedProjectionNameJsonConverter : JsonConverter<ConnectedProjectionName>
    {
        public override bool CanRead => false;

        public override void WriteJson(
            JsonWriter writer,
            ConnectedProjectionName value,
            JsonSerializer serializer)
            => writer.WriteValue(value.ToString());

        public override ConnectedProjectionName ReadJson(
            JsonReader reader,
            Type objectType,
            ConnectedProjectionName existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
            => throw new NotImplementedException();
    }
}
