namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Internal.Configuration
{
    using ConnectedProjections;

    internal interface IConnectedProjectionSettings : IConnectedProjectionCatchUpSettings
    {
        MessageHandlingRetryPolicy RetryPolicy { get; }
    }
}
