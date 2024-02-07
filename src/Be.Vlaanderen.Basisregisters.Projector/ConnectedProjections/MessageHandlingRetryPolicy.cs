namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Internal;
    using Internal.StreamGapStrategies;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore.Streams;

    public abstract class MessageHandlingRetryPolicy
    {
        internal abstract IConnectedProjectionMessageHandler ApplyOn(IConnectedProjectionMessageHandler messageHandler);

        private protected class RetryMessageHandler : IConnectedProjectionMessageHandler
        {
            private readonly Func<IEnumerable<StreamMessage>, IStreamGapStrategy, CancellationToken, Task> _messageHandling;

            public ConnectedProjectionIdentifier Projection { get; }
            public ILogger Logger { get; }

            public RetryMessageHandler(
                Func<IEnumerable<StreamMessage>, IStreamGapStrategy, CancellationToken, Task> messageHandling,
                ConnectedProjectionIdentifier projection,
                ILogger messageHandlerLogger)
            {
                _messageHandling = messageHandling ?? throw new ArgumentNullException(nameof(messageHandling));
                Projection = projection ?? throw new ArgumentNullException(nameof(projection));
                Logger = messageHandlerLogger ?? throw new ArgumentNullException(nameof(messageHandlerLogger));
            }

            public async Task HandleAsync(IEnumerable<StreamMessage> messages, IStreamGapStrategy streamGapStrategy, CancellationToken cancellationToken)
                => await _messageHandling(messages, streamGapStrategy, cancellationToken).NoContext();
        }
    }
}
