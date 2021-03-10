namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    public class RegisteredConnectedProjection
    {
        public ConnectedProjectionIdentifier Id { get; }
        public ConnectedProjectionState State { get; }
        public string Name { get; set; }
        public string Description { get; set; }

        public RegisteredConnectedProjection(
            ConnectedProjectionIdentifier id,
            ConnectedProjectionState state,
            string name,
            string description)
        {
            Id = id;
            State = state;
            Name = name;
            Description = description;
        }
    }
}
