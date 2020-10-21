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

        Task<ProjectionStateItem?> GetProjectionState(
            ConnectedProjectionName projectionName,
            CancellationToken ct);

        Task SetErrorMessage(
            ConnectedProjectionName projectionName,
            Exception exception,
            CancellationToken cancellationToken);

        Task ClearErrorMessage(
            ConnectedProjectionName projectionName,
            CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }

    internal class ConnectedProjectionContext<TContext> : IConnectedProjectionContext<TContext>
        where TContext : RunnerDbContext<TContext>
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
            var state = await GetProjectionState(projectionName, cancellationToken);
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
            var projectionState = await GetProjectionState(projectionName, cancellationToken);
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

        public async Task SetErrorMessage(
            ConnectedProjectionName projectionName,
            Exception exception,
            CancellationToken cancellationToken)
        {
            //exception.ToString() => https://stackoverflow.com/a/2176722/412692
            await _context.SetErrorMessage(projectionName, exception.ToString(), cancellationToken);
        }

        public async Task ClearErrorMessage(
            ConnectedProjectionName projectionName,
            CancellationToken cancellationToken)
            => await _context.SetErrorMessage(projectionName, null, cancellationToken);

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
            => await _context.SaveChangesAsync(cancellationToken);

        public void Dispose()
            => _context.Dispose();

        public ValueTask DisposeAsync()
            => _context.DisposeAsync();

        public async Task<ProjectionStateItem?> GetProjectionState(
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
