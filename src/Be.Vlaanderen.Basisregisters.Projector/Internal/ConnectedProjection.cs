namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac.Features.OwnedInstances;
    using ConnectedProjections;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Connector;
    using ProjectionHandling.Runner;
    using ProjectionHandling.SqlStreamStore;

    internal interface IConnectedProjection
    {
        ConnectedProjectionName Name { get; }
        dynamic Instance { get; }
        Task UpdateUserDesiredState(UserDesiredState userDesiredState, CancellationToken cancellationToken);
        Task<bool> ShouldResume(CancellationToken cancellationToken);
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

        public async Task UpdateUserDesiredState(UserDesiredState userDesiredState, CancellationToken cancellationToken)
        {
            using (var ctx = ContextFactory().Value)
            {
                await ctx.UpdateProjectionDesiredState(Name, userDesiredState, cancellationToken);
                await ctx.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ShouldResume(CancellationToken cancellationToken)
        {
            using (var ctx = ContextFactory().Value)
            {
                var projectionStateItem = await ctx.ProjectionStates.SingleOrDefaultAsync(item => item.Name == Name, cancellationToken);
                return projectionStateItem?.DesiredState == UserDesiredState.Started;
            }
        }

        public dynamic Instance => this;
    }
}
