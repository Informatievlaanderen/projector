namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands.Subscription
{
    using ConnectedProjections;

    internal class Subscribe : SubscriptionCommand
    {
        public ConnectedProjectionName ProjectionName { get; }

        public Subscribe(ConnectedProjectionName projectionName)
        {
            ProjectionName = projectionName;
        }
    }
}
