namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    public class RegisteredConnectedProjection
    {
        public ConnectedProjectionIdentifier Id { get; }
        public ConnectedProjectionState State { get; }

        public RegisteredConnectedProjection(ConnectedProjectionIdentifier id, ConnectedProjectionState state)
        {
            Id = id;
            State = state;
        }
    }
}
