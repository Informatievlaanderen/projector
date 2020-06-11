namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Internal.RetryPolicies;
    using SqlStreamStore.Streams;

    public abstract class MessageHandlingRetryPolicy
    {
        public static MessageHandlingRetryPolicy NoRetries => new NoRetries();

        public static MessageHandlingRetryPolicy ExponentialBackOff<TException>(int numberOfRetries, TimeSpan wait)
            where TException : Exception
            => new ExponentialBackOff<TException>(numberOfRetries, wait);

        public static MessageHandlingRetryPolicy Custom(MessageHandlingRetryPolicy policy) => policy;
        
        public abstract IConnectedProjectionMessageHandler ApplyOn(IConnectedProjectionMessageHandler messageHandler);

        protected IConnectedProjectionMessageHandler CreateMessageHandlerFor(Func<IEnumerable<StreamMessage>, CancellationToken, Task> messageHandling)
            => new MessageHandler(messageHandling);

        private class MessageHandler : IConnectedProjectionMessageHandler
        {
            private readonly Func<IEnumerable<StreamMessage>, CancellationToken, Task> _messageHandling;

            public MessageHandler(Func<IEnumerable<StreamMessage>, CancellationToken, Task> messageHandling)
                => _messageHandling = messageHandling ?? throw new ArgumentNullException(nameof(messageHandling));

            public async Task HandleAsync(IEnumerable<StreamMessage> messages, CancellationToken cancellationToken)
                => await _messageHandling(messages, cancellationToken);
        }
    }
}
