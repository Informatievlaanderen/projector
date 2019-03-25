namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands
{
    using System;
    using System.Threading.Tasks;
    using CatchUp;
    using Microsoft.Extensions.Logging;
    using Runners;
    using Subscription;

    internal class ConnectedProjectionsCommandHandler
    {
        private readonly ConnectedProjectionsCatchUpRunner _catchUpRunner;
        private readonly ConnectedProjectionsSubscriptionRunner _subscriptionRunner;
        private readonly IConnectedProjectionsCommandBus _commandBus;
        private readonly ILogger _logger;


        public ConnectedProjectionsCommandHandler(
            ConnectedProjectionsSubscriptionRunner subscriptionRunner,
            ConnectedProjectionsCatchUpRunner catchUpRunner,
            IConnectedProjectionsCommandBus commandBus,
            ILoggerFactory loggerFactory)
        {
            _subscriptionRunner = subscriptionRunner ?? throw new ArgumentNullException(nameof(subscriptionRunner));
            _catchUpRunner = catchUpRunner ?? throw new ArgumentNullException(nameof(catchUpRunner));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionsManager>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task Handle(ConnectedProjectionCommand command)
        {
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
                    _commandBus.Queue(start.DefaultCommand);
                    break;

                case StartAll _:
                    _commandBus.Queue<SubscribeAll>();
                    break;

                case Stop stop:
                    _commandBus.Queue(new StopCatchUp(stop.ProjectionName));
                    _commandBus.Queue(new Unsubscribe(stop.ProjectionName));
                    break;

                case StopAll _:
                    _commandBus.Queue<StopAllCatchUps>();
                    _commandBus.Queue<UnsubscribeAll>();
                    break;

                default:
                    _logger.LogError("No handler defined for {Command}", command);
                    break;
            }
        }
    }
}
