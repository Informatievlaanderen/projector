namespace Be.Vlaanderen.Basisregisters.Projector.Internal.RetryPolicies
{
    using ConnectedProjections;

    internal class StreamStoreNoRetries : StreamStoreMessageHandlingRetryPolicy
    {
        internal override IStreamStoreConnectedProjectionMessageHandler ApplyOn(IStreamStoreConnectedProjectionMessageHandler messageHandler)
            => messageHandler;
    }

    internal class KafkaNoRetries : KafkaMessageHandlingRetryPolicy
    {
        internal override IKafkaConnectedProjectionMessageHandler ApplyOn(IKafkaConnectedProjectionMessageHandler messageHandler)
            => messageHandler;
    }
}
