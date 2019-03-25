namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands
{
    using ConnectedProjections;

    internal class Stop : ConnectedProjectionCommand
    {
        public ConnectedProjectionName ProjectionName { get; }

        public Stop(ConnectedProjectionName projectionName) => ProjectionName = projectionName;
    }
}
