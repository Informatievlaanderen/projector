namespace Be.Vlaanderen.Basisregisters.Projector.Internal.RetryPolicies
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Microsoft.Extensions.Logging;
    using Polly;
    using SqlStreamStore.Streams;

    internal class LinearBackoff<TException> : MessageHandlingRetryPolicy where TException : Exception
    {
        private readonly int _numberOfRetries;
        private readonly TimeSpan _initialWait;

        public LinearBackoff(int numberOfRetries, TimeSpan initialWait)
        {
            if (numberOfRetries < 1)
                throw new ArgumentException($"{nameof(numberOfRetries)} needs to be at least 1");

            _numberOfRetries = numberOfRetries;
            _initialWait = initialWait;
        }

        internal override IConnectedProjectionMessageHandler ApplyOn(IConnectedProjectionMessageHandler messageHandler)
        {
            var projectionName = messageHandler.RunnerName;
            var messageHandlerLogger = messageHandler.Logger;

            void LogRetryAttempt(Exception exception, TimeSpan waitTime, int attempt, Context context)
                => messageHandlerLogger
                .LogWarning(
                    exception,
                    "Projection '{ProjectionName}' failed. Retry attempt #{RetryAttempt} in {RetryTime} seconds.",
                    projectionName,
                    attempt,
                    waitTime.TotalSeconds);

            async Task ExecuteWithRetryPolicy(IEnumerable<StreamMessage> messages, CancellationToken token)
            {
                await Policy
                    .Handle<TException>()
                    .WaitAndRetryAsync(
                        _numberOfRetries,
                        attempt => _initialWait.Multiply(attempt),
                        LogRetryAttempt)
                    .ExecuteAsync(async ct => await messageHandler.HandleAsync(messages, ct), token);
            }

            return new RetryMessageHandler(ExecuteWithRetryPolicy, projectionName, messageHandlerLogger);
        }
    }
}
