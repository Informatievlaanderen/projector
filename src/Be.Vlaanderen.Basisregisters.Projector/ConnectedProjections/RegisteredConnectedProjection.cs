namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    public class RegisteredConnectedProjection
    {
        public RegisteredConnectedProjection(ConnectedProjectionName name, ConnectedProjectionState state)
        {
            Name = name;
            State = state;
        }

        public ConnectedProjectionName Name { get; }
        public ConnectedProjectionState State { get;  }
    }
}
