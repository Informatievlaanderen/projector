namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Configuration
{
    using ConnectedProjections;

    internal interface IConnectedProjectionSettings : IConnectedProjectionCatchUpSettings
    {
        MessageHandlingRetryPolicy RetryPolicy { get; }
    }
}
