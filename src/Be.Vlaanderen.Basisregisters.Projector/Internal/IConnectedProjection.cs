namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using ConnectedProjections.States;

    internal interface IConnectedProjection : IConnectedProjectionStatus
    {
        Type ConnectedProjectionType { get; }
        Type ContextType { get; }
        void Update(ProjectionState state);
    }
}
