namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Internal.Commands
{
    using ConnectedProjections;

    internal class Start : ConnectedProjectionCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }

        public Start(ConnectedProjectionIdentifier projection) => Projection = projection;
    }
}
