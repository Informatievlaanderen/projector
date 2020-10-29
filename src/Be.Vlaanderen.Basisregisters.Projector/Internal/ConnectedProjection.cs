namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac.Features.OwnedInstances;
    using Commands;
    using Configuration;
    using ConnectedProjections;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Connector;
    using ProjectionHandling.Runner;
    using ProjectionHandling.Runner.ProjectionStates;
    using SqlStreamStore;
    using StreamGapStrategies;

    internal interface IConnectedProjection
    {
        ConnectedProjectionName Name { get; }
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
        ConnectedProjectionName Name { get; }
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
        public ConnectedProjectionName Name => new ConnectedProjectionName(typeof(TConnectedProjection));
        public Func<Owned<IConnectedProjectionContext<TContext>>> ContextFactory { get; }
        public IConnectedProjectionMessageHandler ConnectedProjectionMessageHandler { get; }

        public ConnectedProjection(
            Func<Owned<IConnectedProjectionContext<TContext>>> contextFactory,
            TConnectedProjection connectedProjection,
            IConnectedProjectionSettings settings,
            ILoggerFactory loggerFactory)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

            ConnectedProjectionMessageHandler = new ConnectedProjectionMessageHandler<TContext>(
                Name,
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

        public async Task<ProjectionStateItem?> GetProjectionState(CancellationToken cancellationToken)
        {
            await using (var ctx = ContextFactory().Value)
                return await ctx.GetProjectionState(Name, cancellationToken);
        }

        public async Task SetErrorMessage(Exception exception, CancellationToken cancellationToken)
        {
            await using (var ctx = ContextFactory().Value)
            {
                await ctx.SetErrorMessage(Name, exception, cancellationToken);
                await ctx.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task ClearErrorMessage(CancellationToken cancellationToken)
        {
            await using (var ctx = ContextFactory().Value)
            {
                await ctx.ClearErrorMessage(Name, cancellationToken);
                await ctx.SaveChangesAsync(cancellationToken);
            }
        }

        public dynamic Instance => this;
    }
}
