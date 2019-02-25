namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using ConnectedProjections;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;
    using SqlStreamStore;

    internal class ConnectedProjectionsCatchUpRunner
    {
        private readonly Dictionary<ConnectedProjectionName, CancellationTokenSource> _projectionCatchUps;
        private readonly IReadonlyStreamStore _streamStore;
        private readonly IConnectedProjectionEventBus _eventBus;
        private readonly ILogger<ConnectedProjectionsCatchUpRunner> _logger;

        public ConnectedProjectionsCatchUpRunner(
            IReadonlyStreamStore streamStore,
            ILoggerFactory loggerFactory,
            IConnectedProjectionEventBus eventBus,
            IConnectedProjectionEventHandler connectedProjectionEventHandler)
        {
            _projectionCatchUps = new Dictionary<ConnectedProjectionName, CancellationTokenSource>();
            _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
            _eventBus = eventBus ?? throw  new ArgumentNullException(nameof(eventBus));
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionsCatchUpRunner>() ?? throw new ArgumentNullException(nameof(loggerFactory));


            if(null == connectedProjectionEventHandler)
                throw new ArgumentNullException(nameof(connectedProjectionEventHandler));

            connectedProjectionEventHandler
                .RegisterHandleFor<CatchUpStopped>(message => _projectionCatchUps.Remove(message.Projection));
            connectedProjectionEventHandler
                .RegisterHandleFor<CatchUpFinished>(message => _projectionCatchUps.Remove(message.Projection));
        }

        public bool IsCatchingUp(ConnectedProjectionName connectedProjection)
        {
            return null != connectedProjection && _projectionCatchUps.ContainsKey(connectedProjection);
        }

        public void Start<TContext>(IConnectedProjection<TContext> projection)
            where TContext : RunnerDbContext<TContext>
        {
            if(null == projection || IsCatchingUp(projection.Name))
                return;

            _projectionCatchUps.Add(projection.Name, new CancellationTokenSource());

            var projectionCatchUp = new ConnectedProjectionCatchUp<TContext>(
                projection.Name,
                _streamStore,
                projection.ContextFactory,
                projection.ConnectedProjectionMessageHandler,
                _eventBus,
                _logger);

            TaskRunner.Dispatch(async () => { await projectionCatchUp.CatchUpAsync(_projectionCatchUps[projection.Name].Token); });
        }

        public void Stop(ConnectedProjectionName connectedProjection)
        {
            if (null == connectedProjection)
                return;

            if (IsCatchingUp(connectedProjection))
                _projectionCatchUps[connectedProjection].Cancel();
        }

        public IEnumerable<ConnectedProjectionName> StopAll()
        {
            var stoppedCatchUps = _projectionCatchUps
                .Keys
                .ToReadOnlyList();

            foreach (var projectionName in stoppedCatchUps)
                _projectionCatchUps[projectionName].Cancel();

            return stoppedCatchUps;
        }
    }
}
