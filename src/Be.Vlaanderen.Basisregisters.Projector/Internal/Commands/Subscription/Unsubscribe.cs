namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands.Subscription
{
    using ConnectedProjections;

    internal class Unsubscribe : SubscriptionCommand
    {
        public ConnectedProjectionName ProjectionName { get; }

        public Unsubscribe(ConnectedProjectionName projectionName)
        {
            ProjectionName = projectionName;
        }
    }
}
