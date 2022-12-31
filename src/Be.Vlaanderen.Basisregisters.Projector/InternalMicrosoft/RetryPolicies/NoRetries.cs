namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.RetryPolicies
{
    using ConnectedProjectionsMicrosoft;
    using InternalMicrosoft;

    internal class NoRetries : MessageHandlingRetryPolicy
    {
        internal override IConnectedProjectionMessageHandler ApplyOn(IConnectedProjectionMessageHandler messageHandler)
            => messageHandler;
    }
}
