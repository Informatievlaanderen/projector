namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Messages
{
    using ConnectedProjections;
    using Projector.Messages;

    internal class SubscriptionRequested : ConnectedProjectionEvent
    {
        public ConnectedProjectionName Projection { get; }

        public SubscriptionRequested(ConnectedProjectionName projection)
        {
            Projection = projection;
        }
    }
}
