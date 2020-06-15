namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Internal;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore.Streams;

    public abstract class MessageHandlingRetryPolicy
    {
        internal abstract IConnectedProjectionMessageHandler ApplyOn(IConnectedProjectionMessageHandler messageHandler);

        private protected class RetryMessageHandler : IConnectedProjectionMessageHandler
        {
            private readonly Func<IEnumerable<StreamMessage>, CancellationToken, Task> _messageHandling;

            public ConnectedProjectionName RunnerName { get; }
            public ILogger Logger { get; }

            public RetryMessageHandler(
                Func<IEnumerable<StreamMessage>, CancellationToken, Task> messageHandling,
                ConnectedProjectionName projectionName,
                ILogger messageHandlerLogger)
            {
                _messageHandling = messageHandling ?? throw new ArgumentNullException(nameof(messageHandling));
                RunnerName = projectionName ?? throw new ArgumentNullException(nameof(projectionName));
                Logger = messageHandlerLogger ?? throw new ArgumentNullException(nameof(messageHandlerLogger));
            }

            public async Task HandleAsync(IEnumerable<StreamMessage> messages, CancellationToken cancellationToken)
                => await _messageHandling(messages, cancellationToken);
        }
    }
}
