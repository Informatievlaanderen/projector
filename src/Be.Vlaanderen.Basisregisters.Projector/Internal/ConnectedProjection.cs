namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using Autofac.Features.OwnedInstances;
    using ConnectedProjections;
    using ProjectionHandling.Connector;
    using ProjectionHandling.Runner;

    internal interface IConnectedProjection
    {
        ConnectedProjectionName Name { get; }
        Type ConnectedProjectionType { get; }
        Type ContextType { get; }
    }

    internal class ConnectedProjection<TConnectedProjection, TContext> : IConnectedProjection
        where TConnectedProjection : ConnectedProjection<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        public ConnectedProjectionName Name => new ConnectedProjectionName(ConnectedProjectionType);

        public Type ConnectedProjectionType => typeof(TConnectedProjection);
        public Type ContextType => typeof(TContext);

        public TConnectedProjection Projection { get; }
        public Func<Owned<TContext>> ContextFactory { get; }

        public ConnectedProjection(Func<Owned<TContext>> contextFactory, TConnectedProjection connectedProjection)
        {
            ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            Projection = connectedProjection ?? throw new ArgumentNullException(nameof(connectedProjection));
        }
    }
}
