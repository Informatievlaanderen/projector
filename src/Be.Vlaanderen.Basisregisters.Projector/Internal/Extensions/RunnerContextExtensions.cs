namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Extensions
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Microsoft.EntityFrameworkCore;
    using ProjectionHandling.Runner;

    internal static class RunnerContextExtensions
    {
        public static async Task<long?> GetRunnerPositionAsync<TContext>(
            this TContext context,
            ConnectedProjectionName runnerName,
            CancellationToken cancellationToken)
            where TContext : RunnerDbContext<TContext>
        {
            var runnerPositions = await context
                .ProjectionStates
                .ToListAsync(cancellationToken);

            return runnerPositions
                .SingleOrDefault(p => runnerName.Equals(p.Name))
                ?.Position;
        }

        public static async Task UpdateProjectionStateAsync<TContext>(
            this TContext context,
            ConnectedProjectionName runnerName,
            long position,
            CancellationToken cancellationToken)
            where TContext : RunnerDbContext<TContext>
        {
            await context.UpdateProjectionState(
                runnerName.ToString(),
                position,
                cancellationToken);
        }
    }
}
