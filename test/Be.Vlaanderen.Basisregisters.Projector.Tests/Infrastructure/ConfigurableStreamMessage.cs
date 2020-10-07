namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using SqlStreamStore.Streams;

    public class ConfigurableStreamMessage
    {
        private StreamMessage _message;

        public ConfigurableStreamMessage(StreamMessage message)
            => _message = message;

        public ConfigurableStreamMessage WithPosition(long position)
        {
            _message = new StreamMessage(
                _message.StreamId,
                _message.MessageId,
                _message.StreamVersion,
                position,
                _message.CreatedUtc,
                _message.Type,
                _message.JsonMetadata,
                _message.GetJsonData().GetAwaiter().GetResult());

            return this;
        }

        public static implicit operator StreamMessage(ConfigurableStreamMessage message)
            => message._message;
    }
}
