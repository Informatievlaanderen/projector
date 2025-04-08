namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    [JsonConverter(typeof(ConnectedProjectionCommandJsonConverter))]
    internal abstract class ConnectedProjectionCommand
    {
        public string Serialize() => JsonConvert.SerializeObject(this);

        public override string ToString() => Serialize();

        public override int GetHashCode() => Serialize().ToLowerInvariant().GetHashCode();

        public override bool Equals(object? obj)
        {
            return GetType() == obj?.GetType()
                   && obj is ConnectedProjectionCommand command
                   && string.Equals(Serialize(), command.Serialize(), StringComparison.InvariantCultureIgnoreCase);
        }
    }

    internal class ConnectedProjectionCommandJsonConverter : JsonConverter<ConnectedProjectionCommand>
    {
        public override bool CanRead => false;

        public override void WriteJson(
            JsonWriter writer,
            ConnectedProjectionCommand? value,
            JsonSerializer serializer)
        {
            ArgumentNullException.ThrowIfNull(value);

            var type = value.GetType();

            var payloadProperties = type
                .GetProperties()
                .ToDictionary(info => info.Name, info => info.GetValue(value));

            var rawValue = JsonConvert.SerializeObject(new Dictionary<string, object>{{ type.Name, payloadProperties }});
            writer.WriteRawValue(rawValue);
        }

        public override ConnectedProjectionCommand ReadJson(
            JsonReader reader,
            Type objectType,
            ConnectedProjectionCommand? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
            => throw new NotImplementedException();
    }
}
