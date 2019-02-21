namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections.States
{
    public interface IConnectedProjectionStatus
    {
        ConnectedProjectionName Name { get; }
        ProjectionState State { get; }
    }
}
