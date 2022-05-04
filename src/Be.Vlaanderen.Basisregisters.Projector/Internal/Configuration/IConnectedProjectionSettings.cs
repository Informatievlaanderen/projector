namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Configuration
{
    using ConnectedProjections;

    internal interface IStreamStoreConnectedProjectionSettings : IConnectedProjectionCatchUpSettings
    {
        StreamStoreMessageHandlingRetryPolicy RetryPolicy { get; }
    }

    internal interface IKafkaConnectedProjectionSettings
    {
        KafkaMessageHandlingRetryPolicy RetryPolicy { get; }
    }
}
