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
        ConnectedProjectionIdentifier Id { get; }
        ConnectedProjectionInfo Info { get; }
        dynamic Instance { get; }
        Task UpdateUserDesiredState(UserDesiredState userDesiredState, CancellationToken cancellationToken);
        Task<bool> ShouldResume(CancellationToken cancellationToken);
        Task<ProjectionStateItem?> GetProjectionState(CancellationToken cancellationToken);
        Task SetErrorMessage(Exception exception, CancellationToken cancellationToken);
        Task ClearErrorMessage(CancellationToken cancellationToken);
    }

    internal interface IStreamStoreConnectedProjection<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        ConnectedProjectionIdentifier Id { get; }
        Func<Owned<IConnectedProjectionContext<TContext>>> ContextFactory { get; }
        IStreamStoreConnectedProjectionMessageHandler ConnectedProjectionMessageHandler { get; }
        ConnectedProjectionCatchUp<TContext> CreateCatchUp(
            IReadonlyStreamStore streamStore,
            IConnectedProjectionsCommandBus commandBus,
            IStreamGapStrategy catchUpStreamGapStrategy,
            ILogger logger);
    }

    internal interface IKafkaConnectedProjection<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        ConnectedProjectionIdentifier Id { get; }
        Func<Owned<KafkaConnectedProjectionContext<TContext>>> ContextFactory { get; }
        IKafkaConnectedProjectionMessageHandler ConnectedProjectionMessageHandler { get; }
    }

    internal class StreamStoreConnectedProjection<TConnectedProjection, TContext> : IStreamStoreConnectedProjection<TContext>, IConnectedProjection
        where TConnectedProjection : ConnectedProjection<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        private readonly IStreamStoreConnectedProjectionSettings _settings;
        public ConnectedProjectionIdentifier Id => new ConnectedProjectionIdentifier(typeof(TConnectedProjection));
        public ConnectedProjectionInfo Info { get; }
        public Func<Owned<IConnectedProjectionContext<TContext>>> ContextFactory { get; }
        public IStreamStoreConnectedProjectionMessageHandler ConnectedProjectionMessageHandler { get; }

        public StreamStoreConnectedProjection(
            Func<Owned<IConnectedProjectionContext<TContext>>> contextFactory,
            TConnectedProjection connectedProjection,
            IStreamStoreConnectedProjectionSettings settings,
            ILoggerFactory loggerFactory)
        {
            Info = new ConnectedProjectionInfo(
                typeof(TConnectedProjection).GetProjectionName(),
                typeof(TConnectedProjection).GetProjectionDescription());
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

            ConnectedProjectionMessageHandler = new StreamStoreConnectedProjectionMessageHandler<TContext>(
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

    internal class KafkaConnectedProjection<TConnectedProjection, TContext> : IKafkaConnectedProjection<TContext>, IConnectedProjection
        where TConnectedProjection : ConnectedProjection<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        private readonly IKafkaConnectedProjectionSettings _settings;
        public ConnectedProjectionIdentifier Id => new ConnectedProjectionIdentifier(typeof(TConnectedProjection));
        public ConnectedProjectionInfo Info { get; }
        public Func<Owned<KafkaConnectedProjectionContext<TContext>>> ContextFactory { get; }
        public IKafkaConnectedProjectionMessageHandler ConnectedProjectionMessageHandler { get; }

        public KafkaConnectedProjection(
            Func<Owned<KafkaConnectedProjectionContext<TContext>>> contextFactory,
            TConnectedProjection connectedProjection,
            IKafkaConnectedProjectionSettings settings,
            ILoggerFactory loggerFactory)
        {
            Info = new ConnectedProjectionInfo(
                typeof(TConnectedProjection).GetProjectionName(),
                typeof(TConnectedProjection).GetProjectionDescription());
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

            ConnectedProjectionMessageHandler = new KafkaConnectedProjectionMessageHandler<TContext>(
                Id,
                connectedProjection?.Handlers ?? throw new ArgumentNullException(nameof(connectedProjection)),
                ContextFactory,
                loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))
            ).WithPolicy(_settings.RetryPolicy);
        }

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
