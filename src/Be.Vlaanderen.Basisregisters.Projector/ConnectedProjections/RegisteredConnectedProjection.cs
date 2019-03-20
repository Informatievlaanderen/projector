namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    public class RegisteredConnectedProjection
    {
        public ConnectedProjectionName Name { get; }
        public ConnectedProjectionState State { get;  }

        public RegisteredConnectedProjection(ConnectedProjectionName name, ConnectedProjectionState state)
        {
            Name = name;
            State = state;
        }
    }
}
