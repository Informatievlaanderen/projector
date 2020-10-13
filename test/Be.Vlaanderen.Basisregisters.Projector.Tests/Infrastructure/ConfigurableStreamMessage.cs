namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using System;
    using System.Threading;
    using SqlStreamStore.Streams;

    public class ConfigurableStreamMessage
    {
        private readonly StreamMessage _message;
        private long? _position;
        private DateTime? _created;

        public long Position
        {
            get => _position ?? _message.Position;
            set => _position = value;
        }

        public DateTime CreatedUtc
        {
            get => _created ?? _message.CreatedUtc;
            set => _created = value.ToUniversalTime();
        }

        public ConfigurableStreamMessage(StreamMessage message)
            => _message = message;

        public static implicit operator StreamMessage(ConfigurableStreamMessage message)
            => new StreamMessage(
                message._message.StreamId,
                message._message.MessageId,
                message._message.StreamVersion,
                message.Position,
                message.CreatedUtc,
                message._message.Type,
                message._message.JsonMetadata,
                message._message.GetJsonData(CancellationToken.None).GetAwaiter().GetResult());
    }
}
