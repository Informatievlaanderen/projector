namespace Be.Vlaanderen.Basisregisters.Projector.Internal.StreamGapStrategies
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using ConnectedProjections;
    using SqlStreamStore.Streams;

    internal interface IStreamGapStrategy
    {
        Task HandleMessage(
            StreamMessage message,
            IProcessedStreamState state,
            Func<StreamMessage, CancellationToken, Task> executeProjectMessage,
            ConnectedProjectionName runnerName,
            CancellationToken cancellationToken);

        IStreamGapStrategyConfigurationSettings Settings { get; }
    }
}
