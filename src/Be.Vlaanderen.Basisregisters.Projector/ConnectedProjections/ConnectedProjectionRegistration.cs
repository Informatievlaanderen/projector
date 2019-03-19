namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using Autofac.Features.OwnedInstances;
    using Internal;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Connector;
    using ProjectionHandling.Runner;
    using ProjectionHandling.SqlStreamStore;

    public class ConnectedProjectionRegistrationRegistration<TConnectedProjection, TContext> : IConnectedProjectionRegistration
        where TConnectedProjection : ConnectedProjection<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        private readonly TConnectedProjection _connectedProjection;
        private readonly Func<Owned<TContext>> _contextFactory;

        public ConnectedProjectionRegistrationRegistration(TConnectedProjection connectedProjection, Func<Owned<TContext>> contextFactory)
        {
            _connectedProjection = connectedProjection ?? throw new ArgumentNullException(nameof(connectedProjection));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        IConnectedProjection IConnectedProjectionRegistration.CreateConnectedProjection(EnvelopeFactory envelopeFactory, ILoggerFactory loggerFactory)
            => new ConnectedProjection<TConnectedProjection, TContext>(_contextFactory, _connectedProjection, envelopeFactory, loggerFactory);
    }
}
