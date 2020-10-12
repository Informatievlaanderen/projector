namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac.Features.OwnedInstances;
    using ConnectedProjections;
    using Exceptions;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Connector;
    using ProjectionHandling.Runner;
    using SqlStreamStore.Streams;
    using StreamGapStrategies;

    internal interface IConnectedProjectionMessageHandler
    {
        Task HandleAsync(
            IEnumerable<StreamMessage> messages,
            IStreamGapStrategy streamGapStrategy,
            CancellationToken cancellationToken);

        ConnectedProjectionName RunnerName { get; }
        ILogger Logger { get; }
    }

    internal class ConnectedProjectionMessageHandler<TContext> : IConnectedProjectionMessageHandler
        where TContext : RunnerDbContext<TContext>
    {
        private readonly Func<Owned<IConnectedProjectionContext<TContext>>> _contextFactory;
        private readonly ConnectedProjector<TContext> _projector;

        public ConnectedProjectionName RunnerName { get; }

        public ILogger Logger { get; }

        public ConnectedProjectionMessageHandler(
            ConnectedProjectionName runnerName,
            ConnectedProjectionHandler<TContext>[] handlers,
            Func<Owned<IConnectedProjectionContext<TContext>>> contextFactory,
            ILoggerFactory loggerFactory)
        {
            RunnerName = runnerName;
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _projector = new ConnectedProjector<TContext>(Resolve.WhenEqualToHandlerMessageType(handlers));
            Logger = loggerFactory?.CreateLogger<ConnectedProjectionMessageHandler<TContext>>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task HandleAsync(
            IEnumerable<StreamMessage> messages,
            IStreamGapStrategy streamGapStrategy,
            CancellationToken cancellationToken)
        {
            ActiveProcessedStreamState? processedState = null;
            using (var context = _contextFactory().Value)
            {
                try
                {
                    var completeMessageInProcess = CancellationToken.None;
                    processedState = new ActiveProcessedStreamState(await context.GetProjectionPosition(RunnerName, completeMessageInProcess));

                    async Task ProcessMessage(StreamMessage message, CancellationToken ct)
                    {
                        Logger.LogTrace(
                            "[{RunnerName}] [STREAM {StreamId} AT {Position}] [{Type}] [LATENCY {Latency}]",
                            RunnerName,
                            message.StreamId,
                            message.Position,
                            message.Type,
                            CalculateNotVeryPreciseLatency(message));

                        await context.ApplyProjections(_projector, message, ct);
                        processedState?.UpdateWithProcessed(message);
                    }

                    foreach (var message in messages)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        var processAction = DetermineProcessMessageAction(message, processedState);
                        switch (processAction)
                        {
                            case ProcessMessageAction.Skip:
                                continue;
                            case ProcessMessageAction.HandleGap:
                                await streamGapStrategy.HandleMessage(
                                    message,
                                    processedState,
                                    ProcessMessage,
                                    RunnerName,
                                    completeMessageInProcess);
                                break;
                            case ProcessMessageAction.Process:
                                await ProcessMessage(message, completeMessageInProcess);
                                break;
                            default:
                                throw new NotImplementedException($"No handle defined for {processAction}");
                        }
                    }

                    if (processedState.HasChanged)
                        await context.UpdateProjectionPosition(
                            RunnerName,
                            processedState.Position,
                            completeMessageInProcess);

                    await context.SaveChangesAsync(completeMessageInProcess);
                }
                catch (TaskCanceledException) { }
                catch (Exception exception)
                {
                    throw new ConnectedProjectionMessageHandlingException(exception, RunnerName, processedState);
                }
            }
        }

        private ProcessMessageAction DetermineProcessMessageAction(
            StreamMessage message,
            IProcessedStreamState lastProcessedPosition)
        {
            if (message.Position <= lastProcessedPosition.Position)
                return ProcessMessageAction.Skip;

            if (message.Position == lastProcessedPosition.ExpectedNextPosition)
                return ProcessMessageAction.Process;

            if (message.Position > lastProcessedPosition.ExpectedNextPosition)
                return ProcessMessageAction.HandleGap;

            return ProcessMessageAction.Unknown;
        }

        private enum ProcessMessageAction
        {
            Unknown = 0,
            Skip = 10,
            HandleGap = 20,
            Process = 30,
        }

        // This is not very precise since we could have differing clocks, and should be seen as merely informational
        private static TimeSpan CalculateNotVeryPreciseLatency(StreamMessage message) => DateTime.UtcNow - message.CreatedUtc;
    }
}
