namespace Be.Vlaanderen.Basisregisters.Projector
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A health check for the projections.
    /// When one or more projections are stopped, the health check will return unhealthy.
    /// If you want to control a projection by starting / stopping it. Don't use this health check.
    /// </summary>
    public sealed class ProjectionsHealthCheck : IHealthCheck
    {
        private readonly IConnectedProjectionsManager _projectionsManager;
        private readonly ILogger<ProjectionsHealthCheck> _logger;

        public ProjectionsHealthCheck(
            IConnectedProjectionsManager projectionsManager,
            ILoggerFactory logger)
        {
            _projectionsManager = projectionsManager;
            _logger = logger.CreateLogger<ProjectionsHealthCheck>();
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var registeredProjections = _projectionsManager.GetRegisteredProjections();

            if (registeredProjections.Any(x => x.State == ConnectedProjectionState.Stopped))
            {
                _logger.LogWarning("Projections with status Stopped detected.");
                return Task.FromResult(HealthCheckResult.Unhealthy("One or more projections are stopped."));
            }

            return Task.FromResult(HealthCheckResult.Healthy("All projections are healthy."));
        }
    }
}
