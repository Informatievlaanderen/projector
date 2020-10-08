namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Commands;
    using Commands.CatchUp;
    using ConnectedProjections;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;
    using SqlStreamStore;
    using StreamGapStrategies;

    internal interface IConnectedProjectionsCatchUpRunner
    {
        void HandleCatchUpCommand<TCatchUpCommand>(TCatchUpCommand command)
            where TCatchUpCommand : CatchUpCommand;
    }

    internal class ConnectedProjectionsCatchUpRunner : IConnectedProjectionsCatchUpRunner
    {
        private readonly Dictionary<ConnectedProjectionName, CancellationTokenSource> _projectionCatchUps;
        private readonly IRegisteredProjections _registeredProjections;
        private readonly IReadonlyStreamStore _streamStore;
        private readonly IConnectedProjectionsCommandBus _commandBus;
        private readonly IStreamGapStrategy _catchUpStreamGapStrategy;
        private readonly ILogger _logger;

        public ConnectedProjectionsCatchUpRunner(
            IRegisteredProjections registeredProjections,
            IReadonlyStreamStore streamStore,
            IConnectedProjectionsCommandBus commandBus,
            IStreamGapStrategy catchUpStreamGapStrategy,
            ILoggerFactory loggerFactory)
        {
            _projectionCatchUps = new Dictionary<ConnectedProjectionName, CancellationTokenSource>();

            _registeredProjections = registeredProjections ?? throw new ArgumentNullException(nameof(registeredProjections));
            _registeredProjections.IsCatchingUp = IsCatchingUp;

            _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _catchUpStreamGapStrategy = catchUpStreamGapStrategy ?? throw new ArgumentNullException(nameof(catchUpStreamGapStrategy));
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionsCatchUpRunner>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        private bool IsCatchingUp(ConnectedProjectionName projectionName)
            => projectionName != null && _projectionCatchUps.ContainsKey(projectionName);

        public void HandleCatchUpCommand<TCatchUpCommand>(TCatchUpCommand command)
            where TCatchUpCommand : CatchUpCommand
        {
            _logger.LogTrace("CatchUp: Handling {Command}", command);
            switch (command)
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
                    _logger.LogError("No handler defined for {Command}", command);
                    break;
            }
        }

        private void Handle(StopCatchUp stopCatchUp) => StopCatchUp(stopCatchUp?.ProjectionName);

        private void StopAllCatchUps()
        {
            foreach (var projectionName in _projectionCatchUps.Keys.ToReadOnlyList())
                StopCatchUp(projectionName);
        }

        private void StopCatchUp(ConnectedProjectionName projectionName)
        {
            if (projectionName == null || IsCatchingUp(projectionName) == false)
                return;

            try
            {
                using (var catchUp = _projectionCatchUps[projectionName])
                    catchUp.Cancel();
            }
            catch (KeyNotFoundException) { }
            catch (ObjectDisposedException) { }
        }

        private void Handle(StartCatchUp startCatchUp)
        {
            var projection = _registeredProjections
                .GetProjection(startCatchUp?.ProjectionName)
                ?.Instance;

            Start(projection);
        }

        private void Start<TContext>(IConnectedProjection<TContext> projection)
            where TContext : RunnerDbContext<TContext>
        {
            if (projection == null || _registeredProjections.IsProjecting(projection.Name))
                return;

            _projectionCatchUps.Add(projection.Name, new CancellationTokenSource());

            var projectionCatchUp = new ConnectedProjectionCatchUp<TContext>(
                projection,
                _streamStore,
                _commandBus,
                _catchUpStreamGapStrategy,
                _logger);

            TaskRunner.Dispatch(async () => await projectionCatchUp.CatchUpAsync(_projectionCatchUps[projection.Name].Token));
        }

        private void Handle(RemoveStoppedCatchUp message) => _projectionCatchUps.Remove(message.ProjectionName);
    }
}
