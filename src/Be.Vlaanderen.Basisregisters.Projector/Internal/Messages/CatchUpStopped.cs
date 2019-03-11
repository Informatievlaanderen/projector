namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Messages
{
    using ConnectedProjections;
    using Projector.Messages;

    internal class CatchUpStopped : ConnectedProjectionEvent
    {
        public ConnectedProjectionName Projection { get; }

        public CatchUpStopped(ConnectedProjectionName projection)
        {
            Projection = projection;
        }
    }
}