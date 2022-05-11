namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Runners
{
    using Confluent.Kafka;

    public class ConsumerOptions
    {
        public string ConsumerGroupId { get; }
        public string Topic { get; }
        public Offset? Offset { get; }

        public ConsumerOptions(
            string consumerGroupId,
            string topic,
            Offset? offset)
        {
            ConsumerGroupId = consumerGroupId;
            Topic = topic;
            Offset = offset;
        }
    }
}
