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

        ConnectedProjectionName RunnerName { get; }
        ILogger Logger { get; }
    }

    internal class ConnectedProjectionMessageHandler<TContext> : IConnectedProjectionMessageHandler
        where TContext : RunnerDbContext<TContext>
    {
        private readonly Func<Owned<TContext>> _contextFactory;
        private readonly ConnectedProjector<TContext> _projector;
        private readonly EnvelopeFactory _envelopeFactory;

        public ConnectedProjectionName RunnerName { get; }

        public ILogger Logger { get; }

        public ConnectedProjectionMessageHandler(
            ConnectedProjectionName runnerName,
            ConnectedProjectionHandler<TContext>[] handlers,
            Func<Owned<TContext>> contextFactory,
            EnvelopeFactory envelopeFactory,
            ILoggerFactory loggerFactory)
        {
            RunnerName = runnerName;
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _projector = new ConnectedProjector<TContext>(Resolve.WhenEqualToHandlerMessageType(handlers));
            _envelopeFactory = envelopeFactory ?? throw new ArgumentNullException(nameof(envelopeFactory));
            Logger = loggerFactory?.CreateLogger<ConnectedProjectionMessageHandler<TContext>>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task HandleAsync(
            IEnumerable<StreamMessage> messages,
            CancellationToken cancellationToken)
        {
            ProcessedStreamState? processedState = null;
            using (var context = _contextFactory())
            {
                try
                {
                    var completeMessageInProcess = CancellationToken.None;
                    processedState = new ProcessedStreamState(await context.Value.GetRunnerPositionAsync(RunnerName, completeMessageInProcess));
                    foreach (var message in messages)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        if (message.Position <= processedState.Position)
                            continue;

                        if (message.Position > processedState.ExpectedNextPosition)
                        {
                            var positions = new List<long>();
                            for (var position = processedState.ExpectedNextPosition; position < message.Position; position++)
                                positions.Add(position);

                            Logger.LogWarning(
                                "Expected messages at positions [{unprocessedPositions}] were not processed for {RunnerName}.",
                                string.Join(", ", positions),
                                RunnerName);
                        }

                        if (message.Position == processedState.ExpectedNextPosition)
                        {
                            Logger.LogTrace(
                                "[{RunnerName}] [STREAM {StreamId} AT {Position}] [{Type}] [LATENCY {Latency}]",
                                RunnerName,
                                message.StreamId,
                                message.Position,
                                message.Type,
                                CalculateNotVeryPreciseLatency(message));

                            var envelope = _envelopeFactory.Create(message);
                            await _projector.ProjectAsync(context.Value, envelope, completeMessageInProcess);
                            processedState.UpdateWithProcessed(message);
                        }
                    }

                    if (processedState.HasChanged)
                        await context.Value.UpdateProjectionStateAsync(
                            RunnerName,
                            processedState.Position,
                            completeMessageInProcess);

                    await context.Value.SaveChangesAsync(completeMessageInProcess);
                }
                catch (TaskCanceledException) { }
                catch (Exception exception)
                {
                    throw new ConnectedProjectionMessageHandlingException(exception, RunnerName, processedState);
                }
            }
        }

        // This is not very precise since we could have differing clocks, and should be seen as merely informational
        private static TimeSpan CalculateNotVeryPreciseLatency(StreamMessage message) => DateTime.UtcNow - message.CreatedUtc;
    }
}
