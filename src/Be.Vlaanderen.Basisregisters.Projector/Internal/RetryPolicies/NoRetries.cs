namespace Be.Vlaanderen.Basisregisters.Projector.Internal.RetryPolicies
{
    using System;
    using ConnectedProjections;

    internal class NoRetries : MessageHandlingRetryPolicy
    {
        public override IConnectedProjectionMessageHandler ApplyOn(IConnectedProjectionMessageHandler messageHandler)
            => messageHandler;
    }
}
