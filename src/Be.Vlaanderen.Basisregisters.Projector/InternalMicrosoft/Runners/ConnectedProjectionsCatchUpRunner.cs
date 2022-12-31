namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Commands;
    using ProjectionHandling.Runner.Microsoft;
    using ConnectedProjectionsMicrosoft;
    using StreamGapStrategies;
    using Commands.CatchUp;
    using Extensions;
    using InternalMicrosoft;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore;

    internal interface IConnectedProjectionsCatchUpRunner
    {
        void HandleCatchUpCommand<TCatchUpCommand>(TCatchUpCommand command)
            where TCatchUpCommand : CatchUpCommand;
    }

    internal class ConnectedProjectionsCatchUpRunner : IConnectedProjectionsCatchUpRunner
    {
        private readonly Dictionary<ConnectedProjectionIdentifier, CancellationTokenSource> _projectionCatchUps;
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
            _projectionCatchUps = new Dictionary<ConnectedProjectionIdentifier, CancellationTokenSource>();

            _registeredProjections = registeredProjections ?? throw new ArgumentNullException(nameof(registeredProjections));
            _registeredProjections.IsCatchingUp = IsCatchingUp;

            _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _catchUpStreamGapStrategy = catchUpStreamGapStrategy ?? throw new ArgumentNullException(nameof(catchUpStreamGapStrategy));
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionsCatchUpRunner>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        private bool IsCatchingUp(ConnectedProjectionIdentifier projection)
            => projection != null && _projectionCatchUps.ContainsKey(projection);

        public void HandleCatchUpCommand<TCatchUpCommand>(TCatchUpCommand? command)
            where TCatchUpCommand : CatchUpCommand
        {
            if (command == null)
            {
                _logger.LogWarning("CatchUp: Skipping null Command");
                return;
            }

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

        private void Handle(StopCatchUp stopCatchUp) => StopCatchUp(stopCatchUp.Projection);

        private void StopAllCatchUps()
        {
            foreach (var projection in _projectionCatchUps.Keys.ToReadOnlyList())
                StopCatchUp(projection);
        }

        private void StopCatchUp(ConnectedProjectionIdentifier projection)
        {
            if (projection == null || IsCatchingUp(projection) == false)
                return;

            try
            {
                using (var catchUp = _projectionCatchUps[projection])
                    catchUp.Cancel();
            }
            catch (KeyNotFoundException) { }
            catch (ObjectDisposedException) { }
        }

        private void Handle(StartCatchUp startCatchUp)
        {
            var projection = _registeredProjections
                .GetProjection(startCatchUp.Projection)
                ?.Instance;

            Start(projection);
        }

        private void Start<TContext>(IConnectedProjection<TContext>? projection)
            where TContext : RunnerDbContext<TContext>
        {
            if (projection == null || _registeredProjections.IsProjecting(projection.Id))
                return;

            _projectionCatchUps.Add(projection.Id, new CancellationTokenSource());

            var projectionCatchUp = projection.CreateCatchUp(
                _streamStore,
                _commandBus,
                _catchUpStreamGapStrategy,
                _logger);

            TaskRunner.Dispatch(async () => await projectionCatchUp.CatchUpAsync(_projectionCatchUps[projection.Id].Token));
        }

        private void Handle(RemoveStoppedCatchUp message) => _projectionCatchUps.Remove(message.Projection);
    }
}
