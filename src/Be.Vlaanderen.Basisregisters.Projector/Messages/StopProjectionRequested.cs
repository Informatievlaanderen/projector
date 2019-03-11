namespace Be.Vlaanderen.Basisregisters.Projector.Messages
{
    using ConnectedProjections;
    using Internal;

    public class StopProjectionRequested : ConnectedProjectionEvent
    {
        public ConnectedProjectionName Projection { get; }

        public StopProjectionRequested(ConnectedProjectionName projection)
        {
            Projection = projection;
        }
    }
}