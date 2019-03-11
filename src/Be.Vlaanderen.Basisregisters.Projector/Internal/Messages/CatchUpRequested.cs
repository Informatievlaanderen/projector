namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Messages
{
    using ConnectedProjections;
    using Projector.Messages;

    internal class CatchUpRequested : ConnectedProjectionEvent
    {
        public ConnectedProjectionName Projection { get; }

        public CatchUpRequested(ConnectedProjectionName projection)
        {
            Projection = projection;
        }
    }
}