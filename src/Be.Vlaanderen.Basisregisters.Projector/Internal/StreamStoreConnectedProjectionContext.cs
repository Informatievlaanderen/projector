namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Microsoft.EntityFrameworkCore;
    using ProjectionHandling.Connector;
    using ProjectionHandling.Runner;
    using ProjectionHandling.Runner.ProjectionStates;
    using ProjectionHandling.SqlStreamStore;
    using SqlStreamStore.Streams;

    internal interface IConnectedProjectionContext<TContext> : IDisposable, IAsyncDisposable
        where TContext : RunnerDbContext<TContext>
    {
        Task<long?> GetProjectionPosition(
            ConnectedProjectionIdentifier projection,
            CancellationToken cancellationToken);

        Task UpdateProjectionPosition(
            ConnectedProjectionIdentifier projection,
            long position,
            CancellationToken cancellationToken);

        Task<UserDesiredState?> GetProjectionDesiredState(
            ConnectedProjectionIdentifier projection,
            CancellationToken cancellationToken);

        Task UpdateProjectionDesiredState(
            ConnectedProjectionIdentifier projection,
            UserDesiredState userDesiredState,
            CancellationToken cancellationToken);

        Task ApplyProjections(
            ConnectedProjector<TContext> projector,
            object message,
            CancellationToken ct);

        Task<ProjectionStateItem?> GetProjectionState(
            ConnectedProjectionIdentifier projection,
            CancellationToken ct);

        Task SetErrorMessage(
            ConnectedProjectionIdentifier projection,
            Exception exception,
            CancellationToken cancellationToken);

        Task ClearErrorMessage(
            ConnectedProjectionIdentifier projection,
            CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }

    internal class StreamStoreConnectedProjectionContext<TContext> : IConnectedProjectionContext<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        private readonly TContext _context;
        private readonly EnvelopeFactory _envelopeFactory;

        public StreamStoreConnectedProjectionContext(
            TContext context,
            EnvelopeFactory envelopeFactory)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _envelopeFactory = envelopeFactory ?? throw new ArgumentNullException(nameof(envelopeFactory));
        }

        public async Task<long?> GetProjectionPosition(
            ConnectedProjectionIdentifier projection,
            CancellationToken cancellationToken)
        {
            var state = await GetProjectionState(projection, cancellationToken);
            return state?.Position;
        }

        public async Task UpdateProjectionPosition(
            ConnectedProjectionIdentifier projection,
            long position,
            CancellationToken cancellationToken)
            => await _context
                .UpdateProjectionState(
                    projection.ToString(),
                    position,
                    cancellationToken);

        public async Task<UserDesiredState?> GetProjectionDesiredState(
            ConnectedProjectionIdentifier projection,
            CancellationToken cancellationToken)
        {
            var projectionState = await GetProjectionState(projection, cancellationToken);
            return UserDesiredState.TryParse(projectionState?.DesiredState ?? string.Empty, out var state)
                ? state
                : null;
        }

        public async Task UpdateProjectionDesiredState(
            ConnectedProjectionIdentifier projection,
            UserDesiredState userDesiredState,
            CancellationToken cancellationToken)
            => await _context.UpdateProjectionDesiredState(
                projection,
                userDesiredState,
                CancellationToken.None);

        public async Task ApplyProjections(
            ConnectedProjector<TContext> projector,
            object message,
            CancellationToken cancellationToken)
            => await projector.ProjectAsync(
                _context,
                _envelopeFactory.Create((StreamMessage)message),
                cancellationToken);

        public async Task SetErrorMessage(
            ConnectedProjectionIdentifier projection,
            Exception exception,
            CancellationToken cancellationToken)
        {
            //exception.ToString() => https://stackoverflow.com/a/2176722/412692
            await _context.SetErrorMessage(projection, exception.ToString(), cancellationToken);
        }

        public async Task ClearErrorMessage(
            ConnectedProjectionIdentifier projection,
            CancellationToken cancellationToken)
            => await _context.SetErrorMessage(projection, null, cancellationToken);

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
            => await _context.SaveChangesAsync(cancellationToken);

        public void Dispose()
            => _context.Dispose();

        public ValueTask DisposeAsync()
            => _context.DisposeAsync();

        public async Task<ProjectionStateItem?> GetProjectionState(
            ConnectedProjectionIdentifier projection,
            CancellationToken cancellationToken)
        {
            return await _context
                .ProjectionStates
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.Name == projection.ToString(),
                    cancellationToken);
        }
    }

    internal class KafkaConnectedProjectionContext<TContext> : IConnectedProjectionContext<TContext>
        where TContext : RunnerDbContext<TContext>
    {
        private readonly TContext _context;

        public KafkaConnectedProjectionContext(TContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<long?> GetProjectionPosition(
            ConnectedProjectionIdentifier projection,
            CancellationToken cancellationToken)
        {
            var state = await GetProjectionState(projection, cancellationToken);
            return state?.Position;
        }

        public async Task UpdateProjectionPosition(
            ConnectedProjectionIdentifier projection,
            long position,
            CancellationToken cancellationToken)
            => await _context
                .UpdateProjectionState(
                    projection.ToString(),
                    position,
                    cancellationToken);

        public async Task<UserDesiredState?> GetProjectionDesiredState(
            ConnectedProjectionIdentifier projection,
            CancellationToken cancellationToken)
        {
            var projectionState = await GetProjectionState(projection, cancellationToken);
            return UserDesiredState.TryParse(projectionState?.DesiredState ?? string.Empty, out var state)
                ? state
                : null;
        }

        public async Task UpdateProjectionDesiredState(
            ConnectedProjectionIdentifier projection,
            UserDesiredState userDesiredState,
            CancellationToken cancellationToken)
            => await _context.UpdateProjectionDesiredState(
                projection,
                userDesiredState,
                CancellationToken.None);

        public async Task ApplyProjections(
            ConnectedProjector<TContext> projector,
            object message,
            CancellationToken cancellationToken)
            => await projector.ProjectAsync(
                _context,
                message,
                cancellationToken);

        public async Task SetErrorMessage(
            ConnectedProjectionIdentifier projection,
            Exception exception,
            CancellationToken cancellationToken)
        {
            //exception.ToString() => https://stackoverflow.com/a/2176722/412692
            await _context.SetErrorMessage(projection, exception.ToString(), cancellationToken);
        }

        public async Task ClearErrorMessage(
            ConnectedProjectionIdentifier projection,
            CancellationToken cancellationToken)
            => await _context.SetErrorMessage(projection, null, cancellationToken);

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
            => await _context.SaveChangesAsync(cancellationToken);

        public void Dispose()
            => _context.Dispose();

        public ValueTask DisposeAsync()
            => _context.DisposeAsync();

        public async Task<ProjectionStateItem?> GetProjectionState(
            ConnectedProjectionIdentifier projection,
            CancellationToken cancellationToken)
        {
            return await _context
                .ProjectionStates
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.Name == projection.ToString(),
                    cancellationToken);
        }
    }
}
