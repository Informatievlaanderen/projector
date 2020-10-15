namespace Be.Vlaanderen.Basisregisters.Projector.Internal.StreamGapStrategies
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using ConnectedProjections;
    using Exceptions;
    using SqlStreamStore.Streams;

    internal class DefaultSubscriptionStreamGapStrategy : IStreamGapStrategy
    {
        public IStreamGapStrategyConfigurationSettings Settings { get; }

        public DefaultSubscriptionStreamGapStrategy(IStreamGapStrategyConfigurationSettings settings)
            => Settings = settings ?? throw new ArgumentNullException(nameof(settings));

        public Task HandleMessage(
            StreamMessage message,
            IProcessedStreamState state,
            Func<StreamMessage, CancellationToken, Task> processMessage,
            ConnectedProjectionName runnerName,
            CancellationToken cancellationToken)
            => throw new StreamGapDetectedException(state.DetermineGapPositions(message), runnerName);
    }
}
