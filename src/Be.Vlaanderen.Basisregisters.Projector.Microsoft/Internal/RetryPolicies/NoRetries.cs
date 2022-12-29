namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Internal.RetryPolicies
{
    using ConnectedProjections;

    internal class NoRetries : MessageHandlingRetryPolicy
    {
        internal override IConnectedProjectionMessageHandler ApplyOn(IConnectedProjectionMessageHandler messageHandler)
            => messageHandler;
    }
}
