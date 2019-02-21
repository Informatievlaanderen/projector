namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac.Features.OwnedInstances;
    using ConnectedProjections;
    using Exceptions;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    internal class ConnectedProjectionCatchUp<TContext> where TContext : RunnerDbContext<TContext>
    {
        private readonly ConnectedProjectionMessageHandler<TContext> _messageHandler;
        private readonly ConnectedProjectionName _runnerName;
        private readonly ILogger _logger;

        public int CatchupPageSize { get; set; } = 1000;

        public ConnectedProjectionCatchUp(
            ConnectedProjectionName name,
            ILogger logger,
            ConnectedProjectionMessageHandler<TContext> messageHandler)
        {
            _runnerName = name;
            _logger = logger;
            _messageHandler = messageHandler;
        }

        // Used with reflection, be careful when refactoring/changing
        public async Task CatchUpAsync(
            IReadonlyStreamStore streamStore,
            Func<Owned<TContext>> contextFactory,
            Action startCatchUp,
            Action<CatchUpStopReason> onStopCatchUp,
            CancellationToken cancellationToken)
        {
            void StopCatchUp(CatchUpStopReason reason)
            {
                _logger.LogInformation(
                    "Stopping {RunnerName} CatchUp: {Reason}",
                    _runnerName,
                    reason);

                onStopCatchUp(reason);
            }

            cancellationToken.Register(() => { StopCatchUp(CatchUpStopReason.Aborted); });
            startCatchUp();
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                _logger.LogDebug(
                    "Started catch up with paging (CatchupPageSize: {CatchupPageSize})",
                    CatchupPageSize);

                long? position;
                using (var context = contextFactory())
                    position = await context.Value.GetRunnerPositionAsync(_runnerName, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                _logger.LogInformation(
                    "Start {RunnerName} CatchUp at position: {Position}",
                    _runnerName,
                    position);

                var page = await ReadPages(streamStore, position, cancellationToken);

                var continueProcessing = false == cancellationToken.IsCancellationRequested;
                while (continueProcessing)
                {
                    _logger.LogDebug(
                        "Processing page of {PageSize} starting at POS {FromPosition}",
                        page.Messages.Length,
                        page.FromPosition);

                    await _messageHandler.HandleAsync(page.Messages, contextFactory, cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (page.IsEnd)
                        continueProcessing = false;
                    else
                        page = await page.ReadNext(cancellationToken);
                }

                StopCatchUp(CatchUpStopReason.Finished);
            }
            catch (TaskCanceledException){ }
            catch (ConnectedProjectionMessageHandlingException exception)
            {
                StopCatchUp(CatchUpStopReason.Error);
                _logger.LogError(
                    exception.InnerException,
                    "{RunnerName} catching up failed because an exception was thrown when handling the message at {Position}.",
                    exception.RunnerName,
                    exception.RunnerPosition);
            }
            catch (Exception exception)
            {
                StopCatchUp(CatchUpStopReason.Error);
                _logger.LogError(
                    exception,
                    "{RunnerName} catching up failed because an exception was thrown",
                    _runnerName);
            }
        }

        private async Task<ReadAllPage> ReadPages(
            IReadonlyStreamStore streamStore,
            long? position,
            CancellationToken cancellationToken)
        {
            return await streamStore.ReadAllForwards(
                position + 1 ?? Position.Start,
                CatchupPageSize,
                prefetchJsonData: true,
                cancellationToken);
        }
    }

    internal abstract class ConnectedProjectCatchUpAbstract : ConnectedProjectionCatchUp<ConnectedProjectCatchUpAbstract.AbstractContext>
    {
        private ConnectedProjectCatchUpAbstract(
            ConnectedProjectionName name,
            ILogger logger,
            ConnectedProjectionMessageHandler<AbstractContext> messageHandler)
            : base(name, logger, messageHandler)
        { }

        public static string CatchUpAsyncName = nameof(CatchUpAsync);
        
        public abstract class AbstractContext : RunnerDbContext<AbstractContext>
        {
            public override string ProjectionStateSchema => throw new NotSupportedException();
        }
    }
}

