namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Commands.CatchUp;
    using Commands.Subscription;
    using Exceptions;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using StreamGapStrategies;

    internal class ConnectedProjectionCatchUp<TContext> where TContext : RunnerDbContext<TContext>
    {
        private readonly IConnectedProjection<TContext> _projection;
        private readonly IConnectedProjectionsCommandBus _commandBus;
        private readonly IStreamGapStrategy _catchUpStreamGapStrategy;
        private readonly ILogger _logger;
        private readonly IReadonlyStreamStore _streamStore;

        public int CatchUpPageSize { get; set; } = 1000;

        public ConnectedProjectionCatchUp(
            IConnectedProjection<TContext> projection,
            IReadonlyStreamStore streamStore,
            IConnectedProjectionsCommandBus commandBus,
            IStreamGapStrategy catchUpStreamGapStrategy,
            ILogger logger)
        {
            _projection = projection ?? throw new ArgumentNullException(nameof(projection));
            _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _catchUpStreamGapStrategy = catchUpStreamGapStrategy ?? throw new ArgumentNullException(nameof(catchUpStreamGapStrategy));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task CatchUpAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => CatchUpStopped(CatchUpStopReason.Aborted));

            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                _logger.LogDebug(
                    "Started catch up with paging (CatchUpPageSize: {CatchUpPageSize})",
                    CatchUpPageSize);

                long? position;
                using (var context = _projection.ContextFactory())
                    position = await context.Value.GetProjectionPosition(_projection.Name, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                _logger.LogInformation(
                    "Start catch up {RunnerName} at {Position}",
                    _projection.Name,
                    position);

                var page = await ReadPages(_streamStore, position, cancellationToken);

                var continueProcessing = cancellationToken.IsCancellationRequested == false;
                while (continueProcessing)
                {
                    _logger.LogDebug(
                        "Processing page of {PageSize} starting at POS {FromPosition}",
                        page.Messages.Length,
                        page.FromPosition);

                    await _projection
                        .ConnectedProjectionMessageHandler
                        .HandleAsync(
                            page.Messages,
                            _catchUpStreamGapStrategy,
                            cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (page.IsEnd)
                        continueProcessing = false;
                    else
                        page = await page.ReadNext(cancellationToken);
                }

                CatchUpStopped(CatchUpStopReason.Finished);
            }
            catch (TaskCanceledException) { }
            catch (ConnectedProjectionMessageHandlingException e)
                when(e.InnerException is StreamGapDetectedException)
            {
                var projectionName = e.RunnerName;
                var delayInSeconds = _catchUpStreamGapStrategy.Settings.RetryDelayInSeconds;

                _logger.LogWarning(
                    "Detected gap in the message stream for catching up projection. Aborted projection {_projectionName} and queued restart in {_gapStrategySettings.RetryDelayInSeconds} seconds.",
                    projectionName,
                    delayInSeconds);
                    
                CatchUpStopped(CatchUpStopReason.Aborted);

                _commandBus.Queue(
                    new Restart(
                        projectionName,
                        TimeSpan.FromSeconds(delayInSeconds)));
            }
            catch (ConnectedProjectionMessageHandlingException exception)
            {
                _logger.LogError(
                    exception.InnerException,
                    "{RunnerName} catching up failed because an exception was thrown when handling the message at {Position}.",
                    exception.RunnerName,
                    exception.RunnerPosition);
                    
                CatchUpStopped(CatchUpStopReason.Error);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "{RunnerName} catching up failed because an exception was thrown",
                    _projection.Name);
                    
                CatchUpStopped(CatchUpStopReason.Error);
            }
        }

        private void CatchUpStopped(CatchUpStopReason reason)
        {
            var message = "Stopping catch up {RunnerName}: {Reason}";
            
            switch (reason)
            {
                case CatchUpStopReason.Error:
                    _logger.LogError(message, _projection.Name, reason);
                    break;
                    
                case CatchUpStopReason.Aborted:
                    _logger.LogWarning(message, _projection.Name, reason);
                    break;
                    
                default:
                    _logger.LogInformation(message, _projection.Name, reason);
                    break;
            }

            _commandBus.Queue(new RemoveStoppedCatchUp(_projection.Name));
            
            if (CatchUpStopReason.Finished == reason)
                _commandBus.Queue(new Subscribe(_projection.Name));
        }

        private async Task<ReadAllPage> ReadPages(
            IReadonlyStreamStore streamStore,
            long? position,
            CancellationToken cancellationToken)
        {
            return await streamStore.ReadAllForwards(
                position + 1 ?? Position.Start,
                CatchUpPageSize,
                prefetchJsonData: true,
                cancellationToken);
        }
    }
}

