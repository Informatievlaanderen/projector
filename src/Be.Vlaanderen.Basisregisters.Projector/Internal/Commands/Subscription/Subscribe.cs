namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands.Subscription
{
    internal class Subscribe : SubscriptionCommand
    {
        public IConnectedProjection Projection { get; }

        public Subscribe(IConnectedProjection projection)
        {
            Projection = projection;
        }
    }
}
