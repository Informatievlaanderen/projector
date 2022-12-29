namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Internal.Commands.Subscription
{
    using System;
    using ConnectedProjections;

    internal class Unsubscribe : SubscriptionCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }

        public Unsubscribe(ConnectedProjectionIdentifier projection) => Projection = projection ?? throw new ArgumentNullException(nameof(projection));
    }
}
