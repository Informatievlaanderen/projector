namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.StreamGapStrategies
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjectionsMicrosoft;
    using Configuration;
    using Exceptions;
    using InternalMicrosoft;
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
            ConnectedProjectionIdentifier projection,
            CancellationToken cancellationToken)
            => throw new StreamGapDetectedException(state.DetermineGapPositions(message), projection);
    }
}
