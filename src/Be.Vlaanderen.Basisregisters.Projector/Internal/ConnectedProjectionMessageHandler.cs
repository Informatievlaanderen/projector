namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac.Features.OwnedInstances;
    using ConnectedProjections;
    using Exceptions;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Connector;
    using ProjectionHandling.Runner;
    using ProjectionHandling.SqlStreamStore;
    using SqlStreamStore.Streams;

    internal interface IConnectedProjectionMessageHandler
    {
        Task HandleAsync(
            IEnumerable<StreamMessage> messages,
            CancellationToken cancellationToken);
    }

    internal class ConnectedProjectionMessageHandler<TContext> : IConnectedProjectionMessageHandler
        where TContext : RunnerDbContext<TContext>
    {
        private readonly ConnectedProjectionName _runnerName;
        private readonly Func<Owned<TContext>> _contextFactory;
        private readonly ConnectedProjector<TContext> _projector;
        private readonly EnvelopeFactory _envelopeFactory;
        private readonly ILogger _logger;

        public ConnectedProjectionMessageHandler(
            ConnectedProjectionName runnerName,
            ConnectedProjectionHandler<TContext>[] handlers,
            Func<Owned<TContext>> contextFactory,
            EnvelopeFactory envelopeFactory,
            ILoggerFactory loggerFactory)
        {
            _runnerName = runnerName;
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _projector = new ConnectedProjector<TContext>(Resolve.WhenEqualToHandlerMessageType(handlers));
            _envelopeFactory = envelopeFactory ?? throw new ArgumentNullException(nameof(envelopeFactory));
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionMessageHandler<TContext>>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task HandleAsync(
            IEnumerable<StreamMessage> messages,
            CancellationToken cancellationToken)
        {
            long? lastProcessedMessagePosition = null;
            using (var context = _contextFactory())
            {
                try
                {
                    var completeMessageInProcess = CancellationToken.None;
                    var runnerPosition = await context.Value.GetRunnerPositionAsync(_runnerName, completeMessageInProcess);
                    foreach (var message in messages)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        if (runnerPosition.HasValue && message.Position <= runnerPosition)
                            continue;

                        var expectedPositionToProcess = (lastProcessedMessagePosition ?? runnerPosition ?? -1L) + 1;
                        if (expectedPositionToProcess < message.Position)
                        {
                            var positions = new List<long>();
                            for (var position = expectedPositionToProcess; position < message.Position; position++)
                                positions.Add(position);

                            _logger.LogWarning(
                                "Expected messages at positions [{unprocessedPositions}] were not processed for {RunnerName}.",
                                string.Join(", ", positions),
                                _runnerName);
                        }

                        _logger.LogTrace(
                            "[{RunnerName}] [STREAM {StreamId} AT {Position}] [{Type}] [LATENCY {Latency}]",
                            _runnerName,
                            message.StreamId,
                            message.Position,
                            message.Type,
                            CalculateNotVeryPreciseLatency(message));

                        var envelope = _envelopeFactory.Create(message);
                        await _projector.ProjectAsync(context.Value, envelope, completeMessageInProcess);
                        lastProcessedMessagePosition = message.Position;
                    }

                    if (lastProcessedMessagePosition.HasValue)
                        await context.Value.UpdateProjectionStateAsync(
                            _runnerName,
                            lastProcessedMessagePosition.Value,
                            completeMessageInProcess);

                    await context.Value.SaveChangesAsync(completeMessageInProcess);
                }
                catch (TaskCanceledException) { }
                catch (Exception exception)
                {
                    throw new ConnectedProjectionMessageHandlingException(exception, _runnerName, lastProcessedMessagePosition);
                }
            }
        }

        // This is not very precise since we could have differing clocks, and should be seen as merely informational
        private static TimeSpan CalculateNotVeryPreciseLatency(StreamMessage message) => DateTime.UtcNow - message.CreatedUtc;
    }
}
