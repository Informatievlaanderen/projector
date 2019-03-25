namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    [JsonConverter(typeof(ConnectedProjectionCommandJsonConverter))]
    public abstract class ConnectedProjectionCommand
    {
        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    internal class ConnectedProjectionCommandJsonConverter : JsonConverter<ConnectedProjectionCommand>
    {
        public override bool CanRead => false;

        public override void WriteJson(
            JsonWriter writer,
            ConnectedProjectionCommand value,
            JsonSerializer serializer)
        {
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
            ConnectedProjectionCommand existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
            => throw new NotImplementedException();
    }
}
