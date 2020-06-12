namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Internal;
    using Internal.RetryPolicies;
    using SqlStreamStore.Streams;

    public abstract class MessageHandlingRetryPolicy
    {
        public static MessageHandlingRetryPolicy NoRetries => new NoRetries();

        internal abstract IConnectedProjectionMessageHandler ApplyOn(IConnectedProjectionMessageHandler messageHandler);

        private protected class RetryMessageHandler : IConnectedProjectionMessageHandler
        {
            private readonly Func<IEnumerable<StreamMessage>, CancellationToken, Task> _messageHandling;

            public MessageHandler(Func<IEnumerable<StreamMessage>, CancellationToken, Task> messageHandling)
                => _messageHandling = messageHandling ?? throw new ArgumentNullException(nameof(messageHandling));

            public async Task HandleAsync(IEnumerable<StreamMessage> messages, CancellationToken cancellationToken)
                => await _messageHandling(messages, cancellationToken);
        }
    }
}
