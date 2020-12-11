namespace Be.Vlaanderen.Basisregisters.Projector.Internal.RetryPolicies
{
    using ConnectedProjections;

    public class NoRetries : MessageHandlingRetryPolicy
    {
        internal override IConnectedProjectionMessageHandler ApplyOn(IConnectedProjectionMessageHandler messageHandler)
            => messageHandler;
    }
}
