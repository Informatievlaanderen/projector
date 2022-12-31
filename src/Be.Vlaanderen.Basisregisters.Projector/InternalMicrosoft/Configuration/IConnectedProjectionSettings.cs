namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Configuration
{
    using ConnectedProjectionsMicrosoft;

    internal interface IConnectedProjectionSettings : IConnectedProjectionCatchUpSettings
    {
        MessageHandlingRetryPolicy RetryPolicy { get; }
    }
}
