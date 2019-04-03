namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands
{
    using ConnectedProjections;
    using Subscription;

    internal class Start : ConnectedProjectionCommand
    {
        public ConnectedProjectionName ProjectionName { get; }

        public Start(ConnectedProjectionName projectionName) => ProjectionName = projectionName;
    }
}
