namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using Autofac.Features.OwnedInstances;
    using ConnectedProjections;
    using ConnectedProjections.States;
    using ProjectionHandling.Connector;
    using ProjectionHandling.Runner;

    internal class ConnectedProjection<TConnectedProjection, TContext> : IConnectedProjection, IConnectedProjectionStatus
        where TConnectedProjection : ConnectedProjection<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        public ConnectedProjectionName Name => new ConnectedProjectionName(ConnectedProjectionType);

        public Type ConnectedProjectionType => typeof(TConnectedProjection);
        public Type ContextType => typeof(TContext);

        public ProjectionState State { get; private set; }
        public TConnectedProjection Projection { get; }
        public Func<Owned<TContext>> ContextFactory { get; }

        public ConnectedProjection(Func<Owned<TContext>> contextFactory, TConnectedProjection connectedProjection)
        {
            ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            Projection = connectedProjection ?? throw new ArgumentNullException(nameof(connectedProjection));
            State = ProjectionState.Stopped;
        }

        public void Update(ProjectionState state)
        {
            State = state;
        }
    }
}
