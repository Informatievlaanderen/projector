namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using ConnectedProjections.States;
    using Exceptions;
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner.ProjectionStates;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using SqlStreamStore.Subscriptions;

    internal class ConnectedProjectionsSubscriptionRunner
    {
        private readonly Dictionary<ConnectedProjectionName, Func<StreamMessage, CancellationToken, Task>> _handlers;
        private readonly IReadonlyStreamStore _streamStore;
        private readonly ILogger<ConnectedProjectionsSubscriptionRunner> _logger;
        private readonly Mutex _subscriptionLock;
        private readonly IConnectedProjectionEventBus _eventBus;
        private IAllStreamSubscription _allStreamSubscription;

        public ConnectedProjectionsSubscriptionRunner(
            IReadonlyStreamStore streamStore,
            ILoggerFactory loggerFactory,
            IConnectedProjectionEventBus eventBus)
        {
            _subscriptionLock = new Mutex();
            _handlers = new Dictionary<ConnectedProjectionName, Func<StreamMessage, CancellationToken, Task>>();
            _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionsSubscriptionRunner>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public SubscriptionStreamState SubscriptionsStreamStatus =>
            null == _allStreamSubscription
                ? SubscriptionStreamState.Stopped
                : SubscriptionStreamState.Running;

        private string SubscribedProjectionNames => string.Join(", ", _handlers.Keys);

        public bool HasSubscription(IConnectedProjection connectedProjection)
        {
            return null != connectedProjection && _handlers.ContainsKey(connectedProjection.Name);
        }
        
        public async Task<bool> TrySubscribe(IConnectedProjection connectedProjection, dynamic messageHandler, CancellationToken cancellationToken)
        {
            try
            {
                _subscriptionLock.WaitOne();

                if (null == connectedProjection || null == messageHandler || HasSubscription(connectedProjection) || cancellationToken.IsCancellationRequested)
                    return false;

                var currentPosition = Stop() ?? await _streamStore.ReadHeadPosition(cancellationToken) - BacktrackNumberOfPositions;
                var projectionPosition = await GetProjectionPositionAsync(connectedProjection, cancellationToken);

                var streamIsEmpty = currentPosition < Position.Start;
                var projectionIsUpToDate = projectionPosition.HasValue && projectionPosition >= currentPosition;

                var canSubscribe = (streamIsEmpty || projectionIsUpToDate) &&
                                   false == cancellationToken.IsCancellationRequested;
                if (canSubscribe)
                {
                    _logger.LogInformation(
                        "Add {ProjectionName} to subscriptions",
                        connectedProjection.Name);

                    _handlers.Add(
                        connectedProjection.Name,
                        async (message, token) =>
                        {
                            await messageHandler
                                .HandleAsync(
                                    message,
                                    ((dynamic)connectedProjection).ContextFactory,
                                    token
                                );
                        });
                    connectedProjection.Update(ProjectionState.Subscribed);
                }

                Start(currentPosition);
                return canSubscribe;
            }
            finally
            {
                _subscriptionLock.ReleaseMutex();
            }
        }

        private static async Task<long?> GetProjectionPositionAsync(IConnectedProjection connectedProjection, CancellationToken cancellationToken)
        {
            using (var context = ((dynamic)connectedProjection).ContextFactory())
            {
                var runnerStates = await ((DbSet<ProjectionStateItem>)context.Value.ProjectionStates).ToListAsync(cancellationToken);

                return runnerStates
                    .SingleOrDefault(p => connectedProjection.Name.Equals(p.Name))
                    ?.Position;
            }
        }

        public void Unsubscribe(IConnectedProjection connectedProjection)
        {
            if (null == connectedProjection)
                return;

            if (HasSubscription(connectedProjection))
            {
                var lastPosition = Stop();
                _handlers.Remove(connectedProjection.Name);
                Start(lastPosition);
            }

            if (ProjectionState.Subscribed == connectedProjection.State)
                connectedProjection.Update(ProjectionState.Stopped);
        }

        public IEnumerable<ConnectedProjectionName> UnsubscribeAll()
        {
            var projectionNames = _handlers.Keys.ToList();

            Stop();
            _handlers.Clear();

            return projectionNames;
        }

        private void Start(long? position)
        {
            var alreadyRunning = SubscriptionStreamState.Running == SubscriptionsStreamStatus;
            if (alreadyRunning || false == _handlers.Any())
                return;

            var continueAfterPosition = false == position.HasValue || position < Position.Start ? null : position;
            _logger.LogInformation(
                "Started subscription stream after position: {AfterPosition} for {ProjectionNames}",
                continueAfterPosition,
                SubscribedProjectionNames);

            _allStreamSubscription = _streamStore
                .SubscribeToAll(
                    continueAfterPosition,
                    OnMessageReceived,
                    OnSubscriptionDropped
                );
        }

        private const long BacktrackNumberOfPositions = 100;
        private long? Stop()
        {
            if (null == _allStreamSubscription)
                return null;

            var lastCompletelyProcessedPosition = _allStreamSubscription.LastPosition;
            _allStreamSubscription.Dispose();
            _allStreamSubscription = null;

            if (lastCompletelyProcessedPosition.HasValue)
                _logger.LogInformation(
                    "Stopped subscription stream at position: {AfterPosition}",
                    lastCompletelyProcessedPosition);

            return lastCompletelyProcessedPosition - BacktrackNumberOfPositions;
        }

        private async Task OnMessageReceived(
            IAllStreamSubscription subscription,
            StreamMessage message,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Received message {MessageType} at {Position}, fanning out to projections: {ProjectionNames}.",
                message.Type,
                message.Position,
                SubscribedProjectionNames);

            foreach (var handler in _handlers.Values.ToReadOnlyList())
                await handler(message, cancellationToken);
        }

        private void OnSubscriptionDropped(
            IAllStreamSubscription subscription,
            SubscriptionDroppedReason reason,
            Exception exception)
        {
            if (null == exception || exception is TaskCanceledException)
                return;

            if (exception is ConnectedProjectionMessageHandlingException messageHandlingException)
            {
                _logger.LogError(
                    messageHandlingException.InnerException,
                    "Subscription {RunnerName} failed because an exception was thrown when handling the message at {Position}.",
                    messageHandlingException.RunnerName,
                    messageHandlingException.RunnerPosition);

                _eventBus.Send(new SubscribedProjectionHasThrownAnError(messageHandlingException.RunnerName));
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

