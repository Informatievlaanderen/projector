namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Commands.CatchUp;
    using ConnectedProjections;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;
    using SqlStreamStore;

    internal class ConnectedProjectionsCatchUpRunner
    {
        private readonly Dictionary<ConnectedProjectionName, CancellationTokenSource> _projectionCatchUps;
        private readonly IReadonlyStreamStore _streamStore;
        private readonly IProjectionManager _projectionManager;
        private readonly ILogger<ConnectedProjectionsCatchUpRunner> _logger;

        public ConnectedProjectionsCatchUpRunner(
            IReadonlyStreamStore streamStore,
            ILoggerFactory loggerFactory,
            IProjectionManager projectionManager)
        {
            _projectionCatchUps = new Dictionary<ConnectedProjectionName, CancellationTokenSource>();
            _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
            _projectionManager = projectionManager ?? throw  new ArgumentNullException(nameof(projectionManager));
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionsCatchUpRunner>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public bool IsCatchingUp(ConnectedProjectionName projectionName)
        {
            return null != projectionName && _projectionCatchUps.ContainsKey(projectionName);
        }

        public void HandleCatchUpCommand<TCatchUpEvent>(TCatchUpEvent catchUpEvent)
            where TCatchUpEvent : CatchUpCommand
        {
            switch (catchUpEvent)
            {
                case StartCatchUp startCatchUp:
                    Handle(startCatchUp);
                    break;
                case RemoveStoppedCatchUp removeStoppedCatchUp:
                    Handle(removeStoppedCatchUp);
                    break;
                case StopCatchUp stopCatchUp:
                    Handle(stopCatchUp);
                    break;
                case StopAllCatchUps _:
                    StopAllCatchUps();
                    break;
                default:
                    _logger.LogError("No handler defined for {Message}", catchUpEvent.GetType().Name);
                    break;
            }
        }

        private void Handle(StopCatchUp stopCatchUp)
        {
            if (null == stopCatchUp.ProjectionName)
                return;

            if (IsCatchingUp(stopCatchUp.ProjectionName))
                _projectionCatchUps[stopCatchUp.ProjectionName].Cancel();
        }

        private void StopAllCatchUps()
        {
            foreach (var projectionName in _projectionCatchUps.Keys.ToReadOnlyList())
                _projectionCatchUps[projectionName].Cancel();
        }

        private void Handle(StartCatchUp startCatchUp)
        {
            Start(startCatchUp?.Projection?.Instance);
        }

        private void Start<TContext>(IConnectedProjection<TContext> projection)
            where TContext : RunnerDbContext<TContext>
        {
            if(null == projection || _projectionManager.IsProjecting(projection.Name))
                return;

            _projectionCatchUps.Add(projection.Name, new CancellationTokenSource());

            var projectionCatchUp = new ConnectedProjectionCatchUp<TContext>(
                projection.Name,
                _streamStore,
                projection.ContextFactory,
                projection.ConnectedProjectionMessageHandler,
                _projectionManager,
                _logger);

            TaskRunner.Dispatch(async () => { await projectionCatchUp.CatchUpAsync(_projectionCatchUps[projection.Name].Token); });
        }

        private void Handle(RemoveStoppedCatchUp message)
        {
            _projectionCatchUps.Remove(message.ProjectionName);
        }
    }
}
