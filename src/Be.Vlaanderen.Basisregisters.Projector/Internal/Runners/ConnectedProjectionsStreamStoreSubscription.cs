namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Runners
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Commands.Subscription;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using SqlStreamStore.Subscriptions;

    internal class ConnectedProjectionsStreamStoreSubscription
    {
        private readonly IReadonlyStreamStore _streamStore;
        private readonly IConnectedProjectionsCommandBus _commandBus;
        private readonly ILogger _logger;

        private IAllStreamSubscription _allStreamSubscription;

        public ConnectedProjectionsStreamStoreSubscription(
            IReadonlyStreamStore streamStore,
            IConnectedProjectionsCommandBus commandBus,
            ILoggerFactory loggerFactory)
        {
            _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionsSubscriptionRunner>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public bool StreamIsRunning => _allStreamSubscription != null;
        public long? LastProcessedPosition { get; private set; }

        public async Task Start()
        {
            long? afterPosition = await _streamStore.ReadHeadPosition(CancellationToken.None);
            if (afterPosition < 0)
                afterPosition = null;

            _logger.LogInformation(
                "Started subscription stream after {AfterPosition}",
                afterPosition);

            _allStreamSubscription = _streamStore
                .SubscribeToAll(
                    afterPosition,
                    OnStreamMessageReceived,
                    OnSubscriptionDropped);

            LastProcessedPosition = _allStreamSubscription.LastPosition;
        }

        private Task OnStreamMessageReceived(
            IAllStreamSubscription subscription,
            StreamMessage message,
            CancellationToken cancellationToken)
        {
            LastProcessedPosition = subscription.LastPosition;
            _commandBus.Queue(new ProcessStreamEvent(subscription, message, cancellationToken));

            return Task.CompletedTask;
        }

        private void OnSubscriptionDropped(
            IAllStreamSubscription subscription,
            SubscriptionDroppedReason reason,
            Exception exception)
        {
            _allStreamSubscription = null;

            if (exception == null || exception is TaskCanceledException)
                return;

            _logger.LogError(
                exception,
                "Subscription {SubscriptionName} was dropped. Reason: {Reason}",
                subscription.Name,
                reason);
        }
    }
}
