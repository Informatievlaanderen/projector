namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Runners
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

    internal interface IConnectedProjectionsStreamStoreSubscription
    {
        bool StreamIsRunning { get; }
        Task<long?> Start();
    }

    internal class ConnectedProjectionsStreamStoreSubscription : IConnectedProjectionsStreamStoreSubscription
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
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionsStreamStoreSubscription>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public bool StreamIsRunning => _allStreamSubscription != null;

        public string StreamName => _allStreamSubscription?.Name;

        public async Task<long?> Start()
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

            return afterPosition;
        }

        private Task OnStreamMessageReceived(
            IAllStreamSubscription subscription,
            StreamMessage message,
            CancellationToken cancellationToken)
        {
            _commandBus.Queue(new ProcessStreamEvent(message, cancellationToken));

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
