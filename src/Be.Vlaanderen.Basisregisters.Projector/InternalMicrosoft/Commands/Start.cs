namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Commands
{
    using ConnectedProjectionsMicrosoft;

    internal class Start : ConnectedProjectionCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }

        public Start(ConnectedProjectionIdentifier projection) => Projection = projection;
    }
}
