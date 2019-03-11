namespace Be.Vlaanderen.Basisregisters.Projector.Messages
{
    using ConnectedProjections;
    using Internal;

    public class StartProjectionRequested : ConnectedProjectionEvent
    {
        public ConnectedProjectionName Projection { get; }

        public StartProjectionRequested(ConnectedProjectionName projection)
        {
            Projection = projection;
        }
    }
}