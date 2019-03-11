namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Messages
{
    using ConnectedProjections;
    using Projector.Messages;

    internal class SubscriptionsHasThrownAnError : ConnectedProjectionEvent
    {
        public ConnectedProjectionName ProjectionInError { get; }

        public SubscriptionsHasThrownAnError(ConnectedProjectionName projectionInError)
        {
            ProjectionInError = projectionInError;
        }
    }
}