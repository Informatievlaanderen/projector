namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Commands.Subscription
{
    using System;
    using ConnectedProjectionsMicrosoft;

    internal class Subscribe : SubscriptionCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }

        public Subscribe(ConnectedProjectionIdentifier projection) => Projection = projection ?? throw new ArgumentNullException(nameof(projection));

        public Subscribe Clone() => new Subscribe(Projection);
    }
}
