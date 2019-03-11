namespace Be.Vlaanderen.Basisregisters.Projector.Messages
{
    using System;
    using System.Collections.Generic;
    using ConnectedProjections;
    using Newtonsoft.Json;

    public abstract class ConnectedProjectionEvent
    {
        public override string ToString()
        {
            var jsonSerializerSettings = 
                new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> {new ConnectedProjectionNameConverter()}
                };
            return JsonConvert.SerializeObject(this, jsonSerializerSettings);
        }
        
        private class ConnectedProjectionNameConverter : JsonConverter<ConnectedProjectionName>
        {
            public override void WriteJson(
                JsonWriter writer,
                ConnectedProjectionName value,
                JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override ConnectedProjectionName ReadJson(
                JsonReader reader,
                Type objectType,
                ConnectedProjectionName existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanRead => false;
        }
    }
}