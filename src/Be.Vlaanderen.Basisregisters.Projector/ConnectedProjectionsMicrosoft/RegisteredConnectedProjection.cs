namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjectionsMicrosoft
{
    public class RegisteredConnectedProjection
    {
        public ConnectedProjectionIdentifier Id { get; }
        public ConnectedProjectionState State { get; }
        public ConnectedProjectionInfo Info { get; set; }

        public RegisteredConnectedProjection(
            ConnectedProjectionIdentifier id,
            ConnectedProjectionState state,
            ConnectedProjectionInfo info)
        {
            Id = id;
            State = state;
            Info = info;
        }
    }
}
