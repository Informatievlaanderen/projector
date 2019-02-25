namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections.States
{
    public interface IConnectedProjectionStatus
    {
        ConnectedProjectionName Name { get; }
        ProjectionState State { get; }
    }

    public class ConnectedProjectionStatus : IConnectedProjectionStatus
    {
        public ConnectedProjectionName Name { get; set; }
        public ProjectionState State { get; set; }
    }
}
