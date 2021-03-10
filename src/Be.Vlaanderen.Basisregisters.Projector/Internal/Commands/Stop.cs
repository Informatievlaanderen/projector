namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands
{
    using ConnectedProjections;

    internal class Stop : ConnectedProjectionCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }

        public Stop(ConnectedProjectionIdentifier projection) => Projection = projection;
    }
}
