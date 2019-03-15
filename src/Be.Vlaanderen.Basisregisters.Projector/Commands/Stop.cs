namespace Be.Vlaanderen.Basisregisters.Projector.Commands
{
    using ConnectedProjections;

    public class Stop : ConnectedProjectionCommand
    {
        public ConnectedProjectionName ProjectionName { get; }

        public Stop(ConnectedProjectionName projectionName)
        {
            ProjectionName = projectionName;
        }
    }
}
