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
        ConnectedProjectionIdentifier Projection { get; }
        ILogger Logger { get; }
    }

    internal interface IStreamStoreConnectedProjectionMessageHandler : IConnectedProjectionMessageHandler
    {
        Task HandleAsync(
            IEnumerable<StreamMessage> messages,
            IStreamGapStrategy streamGapStrategy,
            CancellationToken cancellationToken);
    }

    internal interface IKafkaConnectedProjectionMessageHandler : IConnectedProjectionMessageHandler
    {
        Task HandleAsync(
            IEnumerable<object> messages, //TODO: IQueueMessage?
            CancellationToken cancellationToken);
    }

    internal class StreamStoreConnectedProjectionMessageHandler<TContext> : IStreamStoreConnectedProjectionMessageHandler
        where TContext : RunnerDbContext<TContext>
    {
        private readonly Func<Owned<IConnectedProjectionContext<TContext>>> _contextFactory;
        private readonly ConnectedProjector<TContext> _projector;

        public ConnectedProjectionIdentifier Projection { get; }

        public ILogger Logger { get; }

        public StreamStoreConnectedProjectionMessageHandler(
            ConnectedProjectionIdentifier projection,
            ConnectedProjectionHandler<TContext>[] handlers,
            Func<Owned<IConnectedProjectionContext<TContext>>> contextFactory,
            ILoggerFactory loggerFactory)
        {
            Projection = projection;
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _projector = new ConnectedProjector<TContext>(Resolve.WhenEqualToHandlerMessageType(handlers));
            Logger = loggerFactory?.CreateLogger<StreamStoreConnectedProjectionMessageHandler<TContext>>() ?? throw new ArgumentNullException(nameof(loggerFactory));
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
                    processedState = new ActiveProcessedStreamState(await context.GetProjectionPosition(Projection, completeMessageInProcess));

                    async Task ProcessMessage(StreamMessage message, CancellationToken ct)
                    {
                        Logger.LogTrace(
                            "[{Projection}] [STREAM {StreamId} AT {Position}] [{Type}] [LATENCY {Latency}]",
                            Projection,
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
                                    Projection,
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
                            Projection,
                            processedState.Position,
                            completeMessageInProcess);

                    await context.SaveChangesAsync(completeMessageInProcess);
                }
                catch (TaskCanceledException) { }
                catch (Exception exception)
                {
                    await context.SetErrorMessage(Projection, exception, cancellationToken);
                    throw new ConnectedProjectionMessageHandlingException(exception, Projection, processedState);
                }
            }
        }

        private static ProcessMessageAction DetermineProcessMessageAction(
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

    internal class KafkaConnectedProjectionMessageHandler<TContext> : IKafkaConnectedProjectionMessageHandler
        where TContext : RunnerDbContext<TContext>
    {
        private readonly Func<Owned<KafkaConnectedProjectionContext<TContext>>> _contextFactory;
        private readonly ConnectedProjector<TContext> _projector;

        public ConnectedProjectionIdentifier Projection { get; }

        public ILogger Logger { get; }

        public KafkaConnectedProjectionMessageHandler(
            ConnectedProjectionIdentifier projection,
            ConnectedProjectionHandler<TContext>[] handlers,
            Func<Owned<KafkaConnectedProjectionContext<TContext>>> contextFactory,
            ILoggerFactory loggerFactory)
        {
            Projection = projection;
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _projector = new ConnectedProjector<TContext>(Resolve.WhenEqualToHandlerMessageType(handlers));
            Logger = loggerFactory?.CreateLogger<StreamStoreConnectedProjectionMessageHandler<TContext>>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task HandleAsync(
            IEnumerable<object> messages,
            CancellationToken cancellationToken)
        {
            await using var context = _contextFactory().Value;
            try
            {
                var completeMessageInProcess = CancellationToken.None;

                async Task ProcessMessage(object message, CancellationToken ct)
                {
                    await context.ApplyProjections(_projector, message, ct);
                }

                foreach (var message in messages)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await ProcessMessage(message, completeMessageInProcess);
                }

                await context.SaveChangesAsync(completeMessageInProcess);
            }
            catch (TaskCanceledException) { }
            catch (Exception exception)
            {
                await context.SetErrorMessage(Projection, exception, cancellationToken);
                throw new ConnectedProjectionMessageHandlingException(exception, Projection, null);
            }
        }
    }
}
