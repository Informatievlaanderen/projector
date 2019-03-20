namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands.CatchUp
{
    using ConnectedProjections;

    internal class StartCatchUp : CatchUpCommand
    {
        public ConnectedProjectionName ProjectionName { get; }

        public StartCatchUp(ConnectedProjectionName projectionName) => ProjectionName = projectionName;
    }
}
