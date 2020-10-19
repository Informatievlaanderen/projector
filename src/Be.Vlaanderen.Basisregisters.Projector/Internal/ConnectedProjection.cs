namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac.Features.OwnedInstances;
    using ConnectedProjections;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Connector;
    using ProjectionHandling.Runner;

    internal interface IConnectedProjection
    {
        ConnectedProjectionName Name { get; }
        dynamic Instance { get; }
        Task UpdateUserDesiredState(UserDesiredState userDesiredState, CancellationToken cancellationToken);
        Task<bool> ShouldResume(CancellationToken cancellationToken);
        Task<long> GetLastSavedPosition(CancellationToken cancellationToken);
    }

    internal interface IConnectedProjection<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        ConnectedProjectionName Name { get; }
        Func<Owned<IConnectedProjectionContext<TContext>>> ContextFactory { get; }
        IConnectedProjectionMessageHandler ConnectedProjectionMessageHandler { get; }
    }

    internal class ConnectedProjection<TConnectedProjection, TContext> : IConnectedProjection<TContext>, IConnectedProjection
        where TConnectedProjection : ConnectedProjection<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        public ConnectedProjectionName Name => new ConnectedProjectionName(typeof(TConnectedProjection));
        public Func<Owned<IConnectedProjectionContext<TContext>>> ContextFactory { get; }
        public IConnectedProjectionMessageHandler ConnectedProjectionMessageHandler { get; }

        public ConnectedProjection(
            Func<Owned<IConnectedProjectionContext<TContext>>> contextFactory,
            TConnectedProjection connectedProjection,
            MessageHandlingRetryPolicy retryPolicy,
            ILoggerFactory loggerFactory)
        {
            ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

            ConnectedProjectionMessageHandler = new ConnectedProjectionMessageHandler<TContext>(
                Name,
                connectedProjection?.Handlers ?? throw new ArgumentNullException(nameof(connectedProjection)),
                ContextFactory,
                loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))
            ).WithPolicy(retryPolicy);
        }

        public async Task UpdateUserDesiredState(UserDesiredState userDesiredState, CancellationToken cancellationToken)
        {
            await using (var ctx = ContextFactory().Value)
            {
                await ctx.UpdateProjectionDesiredState(Name, userDesiredState, cancellationToken);
                await ctx.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ShouldResume(CancellationToken cancellationToken)
        {
            await using (var ctx = ContextFactory().Value)
            {
                var state = await ctx.GetProjectionDesiredState(Name, cancellationToken);
                return state is { } && state == UserDesiredState.Started;
            }
        }

        public async Task<long> GetLastSavedPosition(CancellationToken cancellationToken)
        {
            await using (var ctx = ContextFactory().Value)
            {
                return (await ctx.ProjectionStates.SingleOrDefaultAsync(x => x.Name == Name, cancellationToken))?.Position ?? -1L;
            }
        }

        public dynamic Instance => this;
    }
}
