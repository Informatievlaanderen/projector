namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Commands
{
    using ConnectedProjectionsMicrosoft;

    internal class Stop : ConnectedProjectionCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }

        public Stop(ConnectedProjectionIdentifier projection) => Projection = projection;
    }
}
