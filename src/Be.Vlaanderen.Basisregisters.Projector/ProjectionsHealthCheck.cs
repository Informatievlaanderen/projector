namespace Be.Vlaanderen.Basisregisters.Projector
{
    using System;
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
        private readonly ProjectionsHealthCheckStrategy _strategy;
        private readonly ILogger<ProjectionsHealthCheck> _logger;

        // TODO: Remove this constructor in a future version.
        [Obsolete("Use the constructor with ProjectionsHealthCheckStrategy instead.")]
        public ProjectionsHealthCheck(
            IConnectedProjectionsManager projectionsManager,
            ILoggerFactory logger)
            : this(new AnyUnhealthyProjectionsHealthCheckStrategy(projectionsManager), logger)
        { }

        public ProjectionsHealthCheck(ProjectionsHealthCheckStrategy strategy, ILoggerFactory logger)
        {
            _strategy = strategy;
            _logger = logger.CreateLogger<ProjectionsHealthCheck>();
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (_strategy.IsHealthy())
            {
                return Task.FromResult(HealthCheckResult.Healthy("All projections are healthy."));
            }

            _logger.LogWarning("Unhealthy projections detected: {UnhealthyMessage}", _strategy.UnhealthyMessage);
            return Task.FromResult(HealthCheckResult.Unhealthy(_strategy.UnhealthyMessage));
        }
    }

    /// <summary>
    /// Strategy to determine the health of the projections.
    /// </summary>
    public abstract class ProjectionsHealthCheckStrategy
    {
        public IConnectedProjectionsManager ProjectionsManager { get; private set; }
        public abstract string UnhealthyMessage { get; }

        public ProjectionsHealthCheckStrategy(IConnectedProjectionsManager projectionsManager)
        {
            ProjectionsManager = projectionsManager;
        }

        public abstract bool IsHealthy();
    }

    /// <summary>
    /// Returns unhealthy if any projection is unhealthy (stopped).
    /// </summary>
    public sealed class AnyUnhealthyProjectionsHealthCheckStrategy : ProjectionsHealthCheckStrategy
    {
        public override string UnhealthyMessage => "One or more projections are stopped.";

        public AnyUnhealthyProjectionsHealthCheckStrategy(IConnectedProjectionsManager projectionsManager)
            : base(projectionsManager)
        { }

        public override bool IsHealthy()
        {
            return ProjectionsManager
                .GetRegisteredProjections()
                .All(x => x.State != ConnectedProjectionState.Stopped);
        }
    }

    /// <summary>
    /// Returns unhealthy if all projections are unhealthy (stopped).
    /// </summary>
    public sealed class AllUnhealthyProjectionsHealthCheckStrategy : ProjectionsHealthCheckStrategy
    {
        public override string UnhealthyMessage => "All projections are stopped.";

        public AllUnhealthyProjectionsHealthCheckStrategy(IConnectedProjectionsManager projectionsManager)
            : base(projectionsManager)
        { }

        public override bool IsHealthy()
        {
            return ProjectionsManager
                .GetRegisteredProjections()
                .Any(x => x.State != ConnectedProjectionState.Stopped);
        }
    }
}
