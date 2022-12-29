namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ProjectionHandling.Connector;
    using ProjectionHandling.Runner.Microsoft;
    using ProjectionHandling.Runner.Microsoft.ProjectionStates;
    using Commands;
    using Configuration;
    using ConnectedProjections;
    using DependencyInjection.OwnedInstances;
    using Extensions;
    using global::Microsoft.Extensions.Logging;
    using SqlStreamStore;
    using StreamGapStrategies;

    internal interface IConnectedProjection
    {
        ConnectedProjectionIdentifier Id { get; }
        ConnectedProjectionInfo Info { get; }
        dynamic Instance { get; }
        Task UpdateUserDesiredState(UserDesiredState userDesiredState, CancellationToken cancellationToken);
        Task<bool> ShouldResume(CancellationToken cancellationToken);
        Task<ProjectionStateItem?> GetProjectionState(CancellationToken cancellationToken);
        Task SetErrorMessage(Exception exception, CancellationToken cancellationToken);
        Task ClearErrorMessage(CancellationToken cancellationToken);
    }

    internal interface IConnectedProjection<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        ConnectedProjectionIdentifier Id { get; }
        Func<Owned<IConnectedProjectionContext<TContext>>> ContextFactory { get; }
        IConnectedProjectionMessageHandler ConnectedProjectionMessageHandler { get; }
        ConnectedProjectionCatchUp<TContext> CreateCatchUp(
            IReadonlyStreamStore streamStore,
            IConnectedProjectionsCommandBus commandBus,
            IStreamGapStrategy catchUpStreamGapStrategy,
            ILogger logger);
    }

    internal class ConnectedProjection<TConnectedProjection, TContext> : IConnectedProjection<TContext>, IConnectedProjection
        where TConnectedProjection : ConnectedProjection<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        private readonly IConnectedProjectionSettings _settings;
        public ConnectedProjectionIdentifier Id => new ConnectedProjectionIdentifier(typeof(TConnectedProjection));
        public ConnectedProjectionInfo Info { get; }
        public Func<Owned<IConnectedProjectionContext<TContext>>> ContextFactory { get; }
        public IConnectedProjectionMessageHandler ConnectedProjectionMessageHandler { get; }

        public ConnectedProjection(
            Func<Owned<IConnectedProjectionContext<TContext>>> contextFactory,
            TConnectedProjection connectedProjection,
            IConnectedProjectionSettings settings,
            ILoggerFactory loggerFactory)
        {
            Info = new ConnectedProjectionInfo(
                typeof(TConnectedProjection).GetProjectionName(),
                typeof(TConnectedProjection).GetProjectionDescription());
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

            ConnectedProjectionMessageHandler = new ConnectedProjectionMessageHandler<TContext>(
                Id,
                connectedProjection?.Handlers ?? throw new ArgumentNullException(nameof(connectedProjection)),
                ContextFactory,
                loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))
            ).WithPolicy(_settings.RetryPolicy);
        }

        public ConnectedProjectionCatchUp<TContext> CreateCatchUp(
            IReadonlyStreamStore streamStore,
            IConnectedProjectionsCommandBus commandBus,
            IStreamGapStrategy catchUpStreamGapStrategy,
            ILogger logger)
            => new ConnectedProjectionCatchUp<TContext>(
                this,
                _settings,
                streamStore,
                commandBus,
                catchUpStreamGapStrategy,
                logger);

        public async Task UpdateUserDesiredState(UserDesiredState userDesiredState, CancellationToken cancellationToken)
        {
            await using (var ctx = ContextFactory().Value)
            {
                await ctx.UpdateProjectionDesiredState(Id, userDesiredState, cancellationToken);
                await ctx.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ShouldResume(CancellationToken cancellationToken)
        {
            await using (var ctx = ContextFactory().Value)
            {
                var state = await ctx.GetProjectionDesiredState(Id, cancellationToken);
                return state is { } && state == UserDesiredState.Started;
            }
        }

        public async Task<ProjectionStateItem?> GetProjectionState(CancellationToken cancellationToken)
        {
            await using (var ctx = ContextFactory().Value)
                return await ctx.GetProjectionState(Id, cancellationToken);
        }

        public async Task SetErrorMessage(Exception exception, CancellationToken cancellationToken)
        {
            await using (var ctx = ContextFactory().Value)
            {
                await ctx.SetErrorMessage(Id, exception, cancellationToken);
                await ctx.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task ClearErrorMessage(CancellationToken cancellationToken)
        {
            await using (var ctx = ContextFactory().Value)
            {
                await ctx.ClearErrorMessage(Id, cancellationToken);
                await ctx.SaveChangesAsync(cancellationToken);
            }
        }

        public dynamic Instance => this;
    }
}
