namespace Be.Vlaanderen.Basisregisters.Projector.Internal.StreamGapStrategies
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using ConnectedProjections;
    using Exceptions;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    internal class DefaultCatchUpStreamGapStrategy : IStreamGapStrategy
    {
        private readonly IReadonlyStreamStore _streamStore;
        private readonly ILogger _logger;
        private readonly IClock _clock;

        public IStreamGapStrategyConfigurationSettings Settings { get; }

        public DefaultCatchUpStreamGapStrategy(
            ILoggerFactory loggerFactory,
            IStreamGapStrategyConfigurationSettings settings,
            IReadonlyStreamStore streamStore,
            IClock clock)
        {
            Settings = settings?? throw new ArgumentNullException(nameof(settings));
            _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _logger = loggerFactory?.CreateLogger<DefaultCatchUpStreamGapStrategy>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task HandleMessage(
            StreamMessage message,
            IProcessedStreamState state,
            Func<StreamMessage, CancellationToken, Task> executeProjectMessage,
            ConnectedProjectionName runnerName,
            CancellationToken cancellationToken)
        {
            if (await IsCloseToStreamEnd(message, cancellationToken))
                throw new StreamGapDetectedException(state.DetermineGapPositions(message), runnerName);

            _logger.LogWarning(
                "Expected messages at positions [{unprocessedPositions}] were not processed for {RunnerName}.",
                string.Join(", ", state.DetermineGapPositions(message)),
                runnerName);

            await executeProjectMessage(message, cancellationToken);
        }

        private async Task<bool> IsCloseToStreamEnd(StreamMessage message, CancellationToken cancellationToken)
        {
            var headPosition = await _streamStore.ReadHeadPosition(cancellationToken);
            var now = _clock
                .GetCurrentInstant()
                .ToDateTimeUtc();
                
            return
                message.CreatedUtc.AddSeconds(Settings.StreamBufferInSeconds) > now &&
                message.Position + Settings.PositionBufferSize > headPosition;
        }
    }
}
