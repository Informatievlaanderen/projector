namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands.CatchUp;
    using Commands.Subscription;
    using ConnectedProjections;
    using Exceptions;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;
    using Projector.Commands;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using SqlStreamStore.Subscriptions;

    internal class ConnectedProjectionsSubscriptionRunner
    {
        private readonly Dictionary<ConnectedProjectionName, Func<StreamMessage, CancellationToken, Task>> _handlers;
        private readonly IReadonlyStreamStore _streamStore;
        private readonly ILogger<ConnectedProjectionsSubscriptionRunner> _logger;
        private readonly IProjectionManager _projectionManager;
        private IAllStreamSubscription _allStreamSubscription;
        private long? _lastProcessedPosition;

        public ConnectedProjectionsSubscriptionRunner(
            IReadonlyStreamStore streamStore,
            ILoggerFactory loggerFactory,
            IProjectionManager projectionManager)
        {
            _handlers = new Dictionary<ConnectedProjectionName, Func<StreamMessage, CancellationToken, Task>>();
            _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionsSubscriptionRunner>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            _projectionManager = projectionManager ?? throw new ArgumentNullException(nameof(projectionManager));
        }

        public bool HasSubscription(ConnectedProjectionName projectionName)
        {
            return null != projectionName && _handlers.ContainsKey(projectionName);
        }

        public async Task HandleSubscriptionCommand<TSubscriptionCommand>(TSubscriptionCommand command)
            where TSubscriptionCommand : SubscriptionCommand
        {
            _logger.LogInformation("Subscription: Handling {Command}", command);
            switch (command)
            {
                case StartSubscriptionStream _:
                    await StartStream();
                    break;
                case ProcessStreamEvent processStreamEvent:
                    await Handle(processStreamEvent);
                    break;
                case Subscribe subscribe:
                    await Handle(subscribe);
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
            if (StreamIsRunning)
                return;

            if (_handlers.Count > 0)
            {
                var staleSubscriptions = _handlers.Keys.ToReadOnlyList();
                _logger.LogInformation("Remove stale subscriptions before starting stream {subscriptions}", staleSubscriptions.ToString(", "));
                _handlers.Clear();
                foreach (var name in staleSubscriptions)
                    await _projectionManager.Send(new Start(name));
            }

            long? afterPosition = await _streamStore.ReadHeadPosition(CancellationToken.None);
            if (afterPosition < 0)
                afterPosition = null;

            _logger.LogInformation(
                "Started subscription stream at position: {AfterPosition}",
                afterPosition);

            _allStreamSubscription = _streamStore
                .SubscribeToAll(
                    afterPosition,
                    OnStreamMessageReceived,
                    OnSubscriptionDropped
                );

            _lastProcessedPosition = _allStreamSubscription.LastPosition;
        }

        private bool StreamIsRunning => null != _allStreamSubscription;

        private async Task Handle(Subscribe subscribe)
        {
            if (StreamIsRunning)
            {
                var projection = _projectionManager
                    .GetProjection(subscribe?.ProjectionName)
                    ?.Instance;
                Subscribe(projection);
            }
            else
            {
                await _projectionManager.Send<StartSubscriptionStream>();
                await _projectionManager.Send(subscribe);
            }
        }

        private void Handle(Unsubscribe unsubscribe)
        {
            if (null == unsubscribe?.ProjectionName)
                return;

            _handlers.Remove(unsubscribe.ProjectionName);
        }

        private void UnsubscribeAll()
        {
            _handlers.Clear();
        }

        private async Task Subscribe<TContext>(IConnectedProjection<TContext> projection)
            where TContext : RunnerDbContext<TContext>
        {
            if (null == projection || _projectionManager.IsProjecting(projection.Name))
                return;

            long? projectionPosition;
            using (var context = projection.ContextFactory())
            {
                projectionPosition = await context.Value.GetRunnerPositionAsync(projection.Name, CancellationToken.None);
            }

            if (null == _lastProcessedPosition)
                throw new Exception("LastPosition should never be unset at this point");

            _logger.LogWarning(
                "Trying to subscribe {Projection} at {ProjectionPosition} on AllStream at {StreamPosition}",
                projection.Name,
                projectionPosition);


            if ((projectionPosition ?? -1) > _lastProcessedPosition || _lastProcessedPosition < Position.Start)
            {
                _logger.LogInformation("Add {ProjectionName} to subscriptions", projection.Name);
                _handlers.Add(
                    projection.Name,
                    async (message, token) =>
                    {
                        await projection.ConnectedProjectionMessageHandler.HandleAsync(message, token);
                    });
            }
            else
                _projectionManager.Send(new StartCatchUp(projection.Name));
        }

        private async Task Handle(ProcessStreamEvent processStreamEvent)
        {
            _logger.LogInformation(
                "Received message {MessageType} at {Position}, fanning out to projections: {ProjectionNames}.",
                processStreamEvent.Message.Type,
                processStreamEvent.Message.Position,
                _handlers.Keys.ToString(", "));

            _lastProcessedPosition = processStreamEvent.Subscription.LastPosition;
            foreach (var handler in _handlers.Values)
                await handler(processStreamEvent.Message, processStreamEvent.CancellationToken);
        }
     
        private async Task OnStreamMessageReceived(IAllStreamSubscription subscription, StreamMessage message, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await _projectionManager.Send(new ProcessStreamEvent(subscription, message, cancellationToken));
        }

        private async void OnSubscriptionDropped(
            IAllStreamSubscription subscription,
            SubscriptionDroppedReason reason,
            Exception exception)
        {
            _allStreamSubscription = null;

            if (null == exception || exception is TaskCanceledException)
                return;

            if (exception is ConnectedProjectionMessageHandlingException messageHandlingException)
            {
                var projectionInError = messageHandlingException.RunnerName;
                _logger.LogError(
                    messageHandlingException.InnerException,
                    "Subscription {RunnerName} failed because an exception was thrown when handling the message at {Position}.",
                    projectionInError,
                    messageHandlingException.RunnerPosition);

                _logger.LogInformation(
                    "Removing faulty subscribed projection {Projection}",
                    projectionInError);
                _handlers.Remove(projectionInError);

                await _projectionManager.Send<StartSubscriptionStream>();
            }
            else
            {
                _logger.LogError(
                    exception,
                    "Subscription {SubscriptionName} was dropped. Reason: {Reason}",
                    subscription.Name,
                    reason);
            }
        }
    }
}
