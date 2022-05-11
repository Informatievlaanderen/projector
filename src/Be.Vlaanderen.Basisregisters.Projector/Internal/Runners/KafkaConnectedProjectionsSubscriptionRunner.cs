namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Commands.Subscription;
    using ConnectedProjections;
    using Exceptions;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;

    internal interface IConnectedProjectionsSubscriptionRunner
    {
        Task HandleSubscriptionCommand<TSubscriptionCommand>(TSubscriptionCommand command)
            where TSubscriptionCommand : SubscriptionCommand;
    }

    internal interface IConnectedProjectionsKafkaSubscriptionRunner<TContext> : IConnectedProjectionsSubscriptionRunner
    { }

    internal class KafkaConnectedProjectionsSubscriptionRunner<TContext> : IConnectedProjectionsKafkaSubscriptionRunner<TContext>
    {
        private readonly Dictionary<ConnectedProjectionIdentifier, Func<object, CancellationToken, Task>> _handlers;
        private readonly IRegisteredProjections _registeredProjections;
        private readonly IConnectedProjectionsKafkaSubscription<TContext> _kafkaSubscription;
        private readonly IConnectedProjectionsCommandBus _commandBus;
        private readonly ILogger _logger;

        public KafkaConnectedProjectionsSubscriptionRunner(
            IRegisteredProjections registeredProjections,
            IConnectedProjectionsKafkaSubscription<TContext> kafkaSubscription,
            IConnectedProjectionsCommandBus commandBus,
            ILoggerFactory loggerFactory)
        {
            _handlers = new Dictionary<ConnectedProjectionIdentifier, Func<object, CancellationToken, Task>>();

            _registeredProjections = registeredProjections ?? throw new ArgumentNullException(nameof(registeredProjections));
            _registeredProjections.IsSubscribed = HasSubscription;

            _kafkaSubscription = kafkaSubscription ?? throw new ArgumentNullException(nameof(kafkaSubscription));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _logger = loggerFactory?.CreateLogger<StreamStoreConnectedProjectionsSubscriptionRunner>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        internal bool HasSubscription(ConnectedProjectionIdentifier projection)
            => projection != null && _handlers.ContainsKey(projection);

        public async Task HandleSubscriptionCommand<TSubscriptionCommand>(TSubscriptionCommand? command)
            where TSubscriptionCommand : SubscriptionCommand
        {
            if (command == null)
            {
                _logger.LogWarning("Subscription: Skipping null Command");
                return;
            }

            _logger.LogTrace("Subscription: Handling {Command}", command);
            switch (command)
            {
                case ProcessKafkaMessage processKafkaMessage:
                    await Handle(processKafkaMessage);
                    break;

                case Subscribe subscribe:
                    await Handle(subscribe);
                    break;

                case SubscribeAll _:
                    await SubscribeAll();
                    break;

                case Unsubscribe unsubscribe:
                    Handle(unsubscribe);
                    break;

                case UnsubscribeAll _:
                    UnsubscribeAll();
                    break;

                default:
                    _logger.LogError("No handler defined for {Command}", command);
                    break;
            }
        }

        private async Task StartStream()
        {
            if (_kafkaSubscription.StreamIsRunning)
                return;

            if (_handlers.Count > 0)
            {
                var staleSubscriptions = _handlers.Keys.ToReadOnlyList();
                _logger.LogInformation("Remove stale subscriptions before starting kafka stream: {subscriptions}", staleSubscriptions.ToString(", "));
                _handlers.Clear();

                foreach (var name in staleSubscriptions)
                    _commandBus.Queue(new Start(name));
            }

            await _kafkaSubscription.Start(CancellationToken.None);
        }

        private async Task Handle(Subscribe subscribe)
        {
            if (_kafkaSubscription.StreamIsRunning)
            {
                var projection = _registeredProjections
                    .GetProjection(subscribe.Projection)
                    ?.Instance;

                await Subscribe(projection);
            }
            else
            {
                await StartStream();
                _commandBus.Queue(subscribe.Clone());
            }
        }

        private async Task SubscribeAll()
        {
            if (_kafkaSubscription.StreamIsRunning)
            {
                foreach (var projection in _registeredProjections.Identifiers)
                    await Handle(new Subscribe(projection));
            }
            else
            {
                await StartStream();
                _commandBus.Queue<SubscribeAll>();
            }
        }

        private void Handle(Unsubscribe unsubscribe)
        {
            _logger.LogInformation("Unsubscribing {Projection}", unsubscribe.Projection);
            _handlers.Remove(unsubscribe.Projection);
        }

        private void UnsubscribeAll()
        {
            _logger.LogInformation("Unsubscribing {Projections}", _handlers.Keys.ToString(", "));
            _handlers.Clear();
        }

        private async Task Subscribe<TContext>(IKafkaConnectedProjection<TContext> projection)
            where TContext : RunnerDbContext<TContext>
        {
            if (projection == null || _registeredProjections.IsProjecting(projection.Id))
                return;

            long? projectionPosition;
            await using (var context = projection.ContextFactory())
                projectionPosition = await context.Value.GetProjectionPosition(projection.Id, CancellationToken.None);

            _logger.LogInformation(
                "Subscribing {Projection} at {ProjectionPosition} to KafkaStream",
                projection.Id,
                projectionPosition);

            _handlers.Add(
                projection.Id,
                async (message, token) => await projection
                    .ConnectedProjectionMessageHandler
                    .HandleAsync(
                        new[] { message },
                        token));
        }

        private async Task Handle(ProcessKafkaMessage processKafkaMessage)
        {
            if (_handlers.Count == 0)
                return;

            _logger.LogTrace(
                "Handling message {MessageType}",
                processKafkaMessage.Message.GetType());

            foreach (var projection in _handlers.Keys.ToReadOnlyList())
            {
                try
                {
                    await _handlers[projection](processKafkaMessage.Message, processKafkaMessage.CancellationToken);
                }
                catch (ConnectedProjectionMessageHandlingException messageHandlingException)
                {
                    var projectionInError = messageHandlingException.Projection;
                    _logger.LogError(
                        messageHandlingException.InnerException,
                        "Handle message Subscription {Projection} failed because an exception was thrown when handling the message at {Position}.",
                        projectionInError,
                        messageHandlingException.RunnerPosition);

                    _logger.LogWarning(
                        "Stopped faulty subscribed projection {Projection}",
                        projectionInError);

                    _handlers.Remove(projectionInError);
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Unhandled exception in Handle:{Command}.", nameof(ProcessStreamEvent));

                    _logger.LogInformation(
                        "Removing faulty subscribed projection {Projection}",
                        projection);

                    _handlers.Remove(projection);
                }
            }
        }
    }
}
