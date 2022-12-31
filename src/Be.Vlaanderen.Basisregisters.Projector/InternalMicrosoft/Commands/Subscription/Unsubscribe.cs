namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Commands.Subscription
{
    using System;
    using ConnectedProjectionsMicrosoft;

    internal class Unsubscribe : SubscriptionCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }

        public Unsubscribe(ConnectedProjectionIdentifier projection) => Projection = projection ?? throw new ArgumentNullException(nameof(projection));
    }
}
