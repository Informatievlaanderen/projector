namespace Be.Vlaanderen.Basisregisters.Projector.Internal.StreamGapStrategies
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore.Streams;

    internal class DefaultCatchUpStreamGapStrategy : IStreamGapStrategy
    {
        private readonly ILogger _logger;

        public DefaultCatchUpStreamGapStrategy(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger<DefaultCatchUpStreamGapStrategy>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task HandleMessage(
            StreamMessage message,
            IProcessedStreamState state,
            Func<StreamMessage, CancellationToken, Task> executeProjectMessage,
            ConnectedProjectionName runnerName,
            CancellationToken cancellationToken)
        {
            _logger.LogWarning(
                "Expected messages at positions [{unprocessedPositions}] were not processed for {RunnerName}.",
                string.Join(", ", state.DetermineGapPositions(message)),
                runnerName);


            await executeProjectMessage(message, cancellationToken);
        }
    }
}
