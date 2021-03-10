namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    public class ConnectedProjectionInfo
    {
        public string Name { get; }
        public string Description { get; }

        public ConnectedProjectionInfo(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
