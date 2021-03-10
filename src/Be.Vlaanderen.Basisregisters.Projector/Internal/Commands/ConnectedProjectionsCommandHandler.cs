namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands
{
    using System;
    using System.Threading.Tasks;
    using CatchUp;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using Runners;
    using Subscription;

    internal interface IConnectedProjectionsCommandHandler
    {
        Task Handle(ConnectedProjectionCommand command);
    }

    internal class ConnectedProjectionsCommandHandler : IConnectedProjectionsCommandHandler
    {
        private readonly IConnectedProjectionsSubscriptionRunner _subscriptionRunner;
        private readonly IConnectedProjectionsCatchUpRunner _catchUpRunner;
        private readonly IConnectedProjectionsCommandBus _commandBus;
        private readonly ILogger _logger;

        public ConnectedProjectionsCommandHandler(
            IConnectedProjectionsSubscriptionRunner subscriptionRunner,
            IConnectedProjectionsCatchUpRunner catchUpRunner,
            IConnectedProjectionsCommandBus commandBus,
            ILoggerFactory loggerFactory)
        {
            _subscriptionRunner = subscriptionRunner ?? throw new ArgumentNullException(nameof(subscriptionRunner));
            _catchUpRunner = catchUpRunner ?? throw new ArgumentNullException(nameof(catchUpRunner));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionsCommandHandler>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task Handle(ConnectedProjectionCommand? command)
        {
            if (command == null)
            {
                _logger.LogWarning("Skipping null Command");
                return;
            }

            switch (command)
            {
                case SubscriptionCommand subscriptionCommand:
                    await _subscriptionRunner.HandleSubscriptionCommand(subscriptionCommand);
                    return;

                case CatchUpCommand catchUpCommand:
                    _catchUpRunner.HandleCatchUpCommand(catchUpCommand);
                    return;
            }

            _logger.LogTrace("Handling {Command}", command);
            switch (command)
            {
                case Start start:
                    _commandBus.Queue(new Subscribe(start.Projection));
                    break;

                case StartAll _:
                    _commandBus.Queue<SubscribeAll>();
                    break;

                case Stop stop:
                    _commandBus.Queue(new StopCatchUp(stop.Projection));
                    _commandBus.Queue(new Unsubscribe(stop.Projection));
                    break;

                case StopAll _:
                    _commandBus.Queue<StopAllCatchUps>();
                    _commandBus.Queue<UnsubscribeAll>();
                    break;

                case Restart restart:
                    await Task.Delay(restart.After);
                    _commandBus.Queue(new Start(restart.Projection));
                    break;

                default:
                    _logger.LogError("No handler defined for {Command}", command);
                    break;
            }
        }
    }
}
