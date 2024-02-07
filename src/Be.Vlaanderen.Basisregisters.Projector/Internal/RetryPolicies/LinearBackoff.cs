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
    using StreamGapStrategies;

    internal class LinearBackOff<TException> : MessageHandlingRetryPolicy where TException : Exception
    {
        private readonly int _numberOfRetries;
        private readonly TimeSpan _delay;

        public LinearBackOff(int numberOfRetries, TimeSpan delay)
        {
            if (numberOfRetries < 1)
                throw new ArgumentException($"{nameof(numberOfRetries)} needs to be at least 1");

            if (delay.CompareTo(TimeSpan.Zero) < 0)
                throw new ArgumentException($"{nameof(delay)} cannot be negative");

            _numberOfRetries = numberOfRetries;
            _delay = delay;
        }

        internal override IConnectedProjectionMessageHandler ApplyOn(IConnectedProjectionMessageHandler messageHandler)
        {
            var projection = messageHandler.Projection;
            var messageHandlerLogger = messageHandler.Logger;

            void LogRetryAttempt(Exception exception, TimeSpan waitTime, int attempt, Context context)
                => messageHandlerLogger
                .LogWarning(
                    exception,
                    "Projection '{Projection}' failed. Retry attempt #{RetryAttempt} in {RetryTime} seconds.",
                    projection,
                    attempt,
                    waitTime.TotalSeconds);

            async Task ExecuteWithRetryPolicy(IEnumerable<StreamMessage> messages, IStreamGapStrategy streamGapStrategy, CancellationToken token)
            {
                await Policy
                    .Handle<TException>()
                    .WaitAndRetryAsync(
                        _numberOfRetries,
                        attempt =>
                        {
                            return _delay.Multiply(attempt);
                        },
                        LogRetryAttempt)
                    .ExecuteAsync(async ct => await messageHandler.HandleAsync(messages, streamGapStrategy, ct).NoContext(), token)
                    .NoContext();
            }

            return new RetryMessageHandler(ExecuteWithRetryPolicy, projection, messageHandlerLogger);
        }
    }
}
