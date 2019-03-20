namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands.CatchUp
{
    using ConnectedProjections;

    internal class RemoveStoppedCatchUp : CatchUpCommand
    {
        public ConnectedProjectionName ProjectionName { get; }

        public RemoveStoppedCatchUp(ConnectedProjectionName projectionName) => ProjectionName = projectionName;
    }
}
