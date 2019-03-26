namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using Autofac.Features.OwnedInstances;
    using ConnectedProjections;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Connector;
    using ProjectionHandling.Runner;
    using ProjectionHandling.SqlStreamStore;

    internal interface IConnectedProjection
    {
        ConnectedProjectionName Name { get; }
        dynamic Instance { get; }
    }

    internal interface IConnectedProjection<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        ConnectedProjectionName Name { get; }
        Func<Owned<TContext>> ContextFactory { get; }
        ConnectedProjectionMessageHandler<TContext> ConnectedProjectionMessageHandler { get; }
    }

    internal class ConnectedProjection<TConnectedProjection, TContext> : IConnectedProjection<TContext>, IConnectedProjection
        where TConnectedProjection : ConnectedProjection<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        public ConnectedProjectionName Name => new ConnectedProjectionName(typeof(TConnectedProjection));
        public Func<Owned<TContext>> ContextFactory { get; }
        public ConnectedProjectionMessageHandler<TContext> ConnectedProjectionMessageHandler { get; }

        public ConnectedProjection(
            Func<Owned<TContext>> contextFactory,
            TConnectedProjection connectedProjection,
            EnvelopeFactory envelopeFactory,
            ILoggerFactory loggerFactory)
        {
            ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

            ConnectedProjectionMessageHandler = new ConnectedProjectionMessageHandler<TContext>(
                Name,
                connectedProjection?.Handlers ?? throw new ArgumentNullException(nameof(connectedProjection)),
                ContextFactory,
                envelopeFactory ?? throw new ArgumentNullException(nameof(envelopeFactory)),
                loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory)));
        }

        public dynamic Instance => this;
    }
}
