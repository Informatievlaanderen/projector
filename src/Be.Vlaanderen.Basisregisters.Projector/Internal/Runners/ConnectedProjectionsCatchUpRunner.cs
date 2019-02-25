namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using ConnectedProjections;
    using Extensions;
    using Microsoft.Extensions.Logging;
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

        public void Start(
            IConnectedProjection connectedProjection,
            dynamic messageHandler)
        {
            if(null == connectedProjection || null == messageHandler || IsCatchingUp(connectedProjection.Name))
                return;

            var contextType = connectedProjection.ContextType;
            var connectedCatchUpType = typeof(ConnectedProjectionCatchUp<>).MakeGenericType(contextType);

            var projectionCatchUp = Activator.CreateInstance(
                connectedCatchUpType,
                connectedProjection.Name,
                _logger,
                messageHandler,
                _eventBus);

            var methodInfo = projectionCatchUp.GetType().GetMethod(ConnectedProjectCatchUpAbstract.CatchUpAsyncName);

            _projectionCatchUps.Add(connectedProjection.Name, new CancellationTokenSource());

            void RunCatchUp()
            {
                methodInfo.Invoke(
                    projectionCatchUp,
                    new object[]
                    {
                        _streamStore,
                        ((dynamic) connectedProjection).ContextFactory,
                        _projectionCatchUps[connectedProjection.Name].Token,
                    });
            }

            TaskRunner.Dispatch(RunCatchUp);
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
