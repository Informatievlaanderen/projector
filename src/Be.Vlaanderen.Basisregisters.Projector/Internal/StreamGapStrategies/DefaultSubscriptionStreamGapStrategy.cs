namespace Be.Vlaanderen.Basisregisters.Projector.Internal.StreamGapStrategies
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using SqlStreamStore.Streams;

    internal class DefaultSubscriptionStreamGapStrategy : IStreamGapStrategy
    {
        private readonly DefaultCatchUpStreamGapStrategy _toRemove_existingWarnAndProcess_Strategy;

        public DefaultSubscriptionStreamGapStrategy(DefaultCatchUpStreamGapStrategy toRemove_existingWarnAndProcess_Strategy)
            => _toRemove_existingWarnAndProcess_Strategy = toRemove_existingWarnAndProcess_Strategy;


        public async Task HandleMessage(
            StreamMessage message,
            IProcessedStreamState state,
            Func<StreamMessage, CancellationToken, Task> processMessage,
            ConnectedProjectionName runnerName,
            CancellationToken cancellationToken)
        {
            // for now: keep the existing warn_and_process behavior
            await _toRemove_existingWarnAndProcess_Strategy.HandleMessage(message, state, processMessage, runnerName, cancellationToken);

            // ToDo: remove the old behavior and replace by throwing an MissingStreamMessagesException
            //throw new MissingStreamMessagesException(state.DetermineGapPositions(message), runnerName);
        }
    }
}
