namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands.CatchUp
{
    using ConnectedProjections;

    internal class StopCatchUp : CatchUpCommand
    {
        public ConnectedProjectionName ProjectionName { get; }

        public StopCatchUp(ConnectedProjectionName projectionName) => ProjectionName = projectionName;
    }
}
