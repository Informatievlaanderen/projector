namespace Be.Vlaanderen.Basisregisters.Projector.Internal.StreamGapStrategies
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Exceptions;
    using SqlStreamStore.Streams;

    internal class DefaultSubscriptionStreamGapStrategy : IStreamGapStrategy
    {
        public async Task HandleMessage(
            StreamMessage message,
            IProcessedStreamState state,
            Func<StreamMessage, CancellationToken, Task> processMessage,
            ConnectedProjectionName runnerName,
            CancellationToken cancellationToken)
            => throw new MissingStreamMessagesException(state.DetermineGapPositions(message), runnerName);
    }
}
