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
            ConnectedProjectionName projectionName,
            CancellationToken cancellationToken);

        Task UpdateProjectionPosition(
            ConnectedProjectionName projectionName,
            long position,
            CancellationToken cancellationToken);

        Task<UserDesiredState?> GetProjectionDesiredState(
            ConnectedProjectionName projectionName,
            CancellationToken cancellationToken);

        Task UpdateProjectionDesiredState(
            ConnectedProjectionName projectionName,
            UserDesiredState userDesiredState,
            CancellationToken cancellationToken);

        Task ApplyProjections(
            ConnectedProjector<TContext> projector,
            StreamMessage message,
            CancellationToken ct);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }

    internal class ConnectedProjectionContext<TContext> : IConnectedProjectionContext<TContext>
        where TContext: RunnerDbContext<TContext>
    {
        private readonly TContext _context;
        private readonly EnvelopeFactory _envelopeFactory;

        public ConnectedProjectionContext(
            TContext context,
            EnvelopeFactory envelopeFactory)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _envelopeFactory = envelopeFactory ?? throw new ArgumentNullException(nameof(envelopeFactory));
        }

        public async Task<long?> GetProjectionPosition(
            ConnectedProjectionName projectionName,
            CancellationToken cancellationToken)
        {
            var state = await GetState(projectionName, cancellationToken);
            return state?.Position;
        }

        public async Task UpdateProjectionPosition(
            ConnectedProjectionName projectionName,
            long position,
            CancellationToken cancellationToken)
            => await _context
                .UpdateProjectionState(
                    projectionName.ToString(),
                    position,
                    cancellationToken);

        public async Task<UserDesiredState?> GetProjectionDesiredState(
            ConnectedProjectionName projectionName,
            CancellationToken cancellationToken)
        {
            var projectionState = await GetState(projectionName, cancellationToken);
            return UserDesiredState.TryParse(projectionState?.DesiredState ?? string.Empty, out var state)
                ? state
                : null;
        }

        public async Task UpdateProjectionDesiredState(
            ConnectedProjectionName projectionName,
            UserDesiredState userDesiredState,
            CancellationToken cancellationToken)
            => await _context.UpdateProjectionDesiredState(
                projectionName,
                userDesiredState,
                CancellationToken.None);

        public async Task ApplyProjections(
            ConnectedProjector<TContext> projector,
            StreamMessage message,
            CancellationToken cancellationToken)
            => await projector.ProjectAsync(
                _context,
                _envelopeFactory.Create(message),
                cancellationToken);

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
            => await _context.SaveChangesAsync(cancellationToken);

        public void Dispose()
            => _context.Dispose();

        public ValueTask DisposeAsync()
            => _context.DisposeAsync();
            
        private async Task<ProjectionStateItem?> GetState(
            ConnectedProjectionName projectionName,
            CancellationToken cancellationToken)
        {
            return await _context
                .ProjectionStates
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.Name == projectionName.ToString(),
                    cancellationToken);
        }
    }
}
