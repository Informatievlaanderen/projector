namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.StreamGapStrategies
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjectionsMicrosoft;
    using Configuration;
    using SqlStreamStore.Streams;

    internal interface IStreamGapStrategy
    {
        Task HandleMessage(
            StreamMessage message,
            IProcessedStreamState state,
            Func<StreamMessage, CancellationToken, Task> executeProjectMessage,
            ConnectedProjectionIdentifier projection,
            CancellationToken cancellationToken);

        IStreamGapStrategyConfigurationSettings Settings { get; }
    }
}
