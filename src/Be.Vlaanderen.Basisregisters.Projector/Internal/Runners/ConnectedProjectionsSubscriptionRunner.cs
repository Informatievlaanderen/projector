namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Commands.CatchUp;
    using Commands.Subscription;
    using ConnectedProjections;
    using Exceptions;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;
    using SqlStreamStore.Streams;
    using StreamGapStrategies;

    internal interface IConnectedProjectionsSubscriptionRunner
    {
        Task HandleSubscriptionCommand<TSubscriptionCommand>(TSubscriptionCommand command)
            where TSubscriptionCommand : SubscriptionCommand;
    }

    internal class ConnectedProjectionsSubscriptionRunner : IConnectedProjectionsSubscriptionRunner
    {
        private readonly Dictionary<ConnectedProjectionIdentifier, Func<StreamMessage, CancellationToken, Task>> _handlers;
        private readonly IRegisteredProjections _registeredProjections;
        private readonly IConnectedProjectionsStreamStoreSubscription _streamsStoreSubscription;
        private readonly IConnectedProjectionsCommandBus _commandBus;
        private readonly IStreamGapStrategy _subscriptionStreamGapStrategy;
        private readonly ILogger _logger;

        private long? _lastProcessedMessagePosition;

        public ConnectedProjectionsSubscriptionRunner(
            IRegisteredProjections registeredProjections,
            IConnectedProjectionsStreamStoreSubscription streamsStoreSubscription,
            IConnectedProjectionsCommandBus commandBus,
            IStreamGapStrategy subscriptionStreamGapStrategy,
            ILoggerFactory loggerFactory)
        {
            _handlers = new Dictionary<ConnectedProjectionIdentifier, Func<StreamMessage, CancellationToken, Task>>();

            _registeredProjections = registeredProjections ?? throw new ArgumentNullException(nameof(registeredProjections));
            _registeredProjections.IsSubscribed = HasSubscription;

            _streamsStoreSubscription = streamsStoreSubscription ?? throw new ArgumentNullException(nameof(streamsStoreSubscription));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _subscriptionStreamGapStrategy = subscriptionStreamGapStrategy ?? throw new ArgumentNullException(nameof(subscriptionStreamGapStrategy));
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionsSubscriptionRunner>() ?? throw new ArgumentNullException(nameof(loggerFactory));
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
                case ProcessStreamEvent processStreamEvent:
                    await Handle(processStreamEvent).NoContext();
                    break;

                case Subscribe subscribe:
                    await Handle(subscribe).NoContext();
                    break;

                case SubscribeAll _:
                    await SubscribeAll().NoContext();
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
            if (_streamsStoreSubscription.StreamIsRunning)
                return;

            if (_handlers.Count > 0)
            {
                var staleSubscriptions = _handlers.Keys.ToReadOnlyList();
                _logger.LogInformation("Remove stale subscriptions before starting stream: {subscriptions}", staleSubscriptions.ToString(", "));
                _handlers.Clear();

                foreach (var name in staleSubscriptions)
                    _commandBus.Queue(new Start(name));
            }

            _lastProcessedMessagePosition = await _streamsStoreSubscription.Start().NoContext();
        }

        private async Task Handle(Subscribe subscribe)
        {
            if (_streamsStoreSubscription.StreamIsRunning)
            {
                var projection = _registeredProjections
                    .GetProjection(subscribe.Projection)
                    ?.Instance;

                await Subscribe(projection);
            }
            else
            {
                await StartStream().NoContext();
                _commandBus.Queue(subscribe.Clone());
            }
        }

        private async Task SubscribeAll()
        {
            if (_streamsStoreSubscription.StreamIsRunning)
            {
                foreach (var projection in _registeredProjections.Identifiers)
                    await Handle(new Subscribe(projection)).NoContext();
            }
            else
            {
                await StartStream().NoContext();
                _commandBus.Queue<SubscribeAll>();
            }
        }

        private void Handle(Unsubscribe unsubscribe)
        {
            _logger.LogInformation("Unsubscribing {Projection}", unsubscribe.Projection);
            _handlers.Remove(unsubscribe.Projection);
            if (_handlers.Count == 0)
                _lastProcessedMessagePosition = null;
        }

        private void UnsubscribeAll()
        {
            _logger.LogInformation("Unsubscribing {Projections}", _handlers.Keys.ToString(", "));
            _handlers.Clear();
            _lastProcessedMessagePosition = null;
        }

        private async Task Subscribe<TContext>(IConnectedProjection<TContext> projection)
            where TContext : RunnerDbContext<TContext>
        {
            if (projection == null || _registeredProjections.IsProjecting(projection.Id))
                return;

            long? projectionPosition;
            await using (var context = projection.ContextFactory().Value)
                projectionPosition = await context.GetProjectionPosition(projection.Id, CancellationToken.None).NoContext();

            if ((projectionPosition ?? -1) >= (_lastProcessedMessagePosition ?? -1))
            {
                _logger.LogInformation(
                    "Subscribing {Projection} at {ProjectionPosition} to AllStream at {StreamPosition}",
                    projection.Id,
                    projectionPosition,
                    _lastProcessedMessagePosition);

                _handlers.Add(
                    projection.Id,
                    async (message, token) => await projection
                        .ConnectedProjectionMessageHandler
                        .HandleAsync(
                            new []{ message },
                            _subscriptionStreamGapStrategy,
                            token).NoContext());
            }
            else
                _commandBus.Queue(new StartCatchUp(projection.Id));
        }

        private async Task Handle(ProcessStreamEvent processStreamEvent)
        {
            if (_handlers.Count == 0)
                return;

            _lastProcessedMessagePosition = processStreamEvent.Message.Position;

            _logger.LogTrace(
                "Handling message {MessageType} at {Position}",
                processStreamEvent.Message.Type,
                processStreamEvent.Message.Position);


            foreach (var projection in _handlers.Keys.ToReadOnlyList())
            {
                try
                {
                    await _handlers[projection](processStreamEvent.Message, processStreamEvent.CancellationToken).NoContext();
                }
                catch (ConnectedProjectionMessageHandlingException e)
                    when (e.InnerException is StreamGapDetectedException)
                {
                    var projectionInError = e.Projection;
                    _logger.LogWarning(
                        "Detected gap in the message stream for subscribed projection. Unsubscribed projection {Projection} and queued restart in {RestartDelay} seconds.",
                        projectionInError,
                        _subscriptionStreamGapStrategy.Settings.RetryDelayInSeconds);

                    _handlers.Remove(projectionInError);

                    _commandBus.Queue(
                        new Restart(
                            projectionInError,
                            TimeSpan.FromSeconds(_subscriptionStreamGapStrategy.Settings.RetryDelayInSeconds)));
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
