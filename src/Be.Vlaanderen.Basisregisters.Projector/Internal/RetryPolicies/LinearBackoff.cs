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

    internal interface ILinearBackOff : IHandlingRetryPolicy { }

    internal class StreamStoreLinearBackOff<TException> : StreamStoreMessageHandlingRetryPolicy, ILinearBackOff
        where TException : Exception
    {
        private readonly int _numberOfRetries;
        private readonly TimeSpan _delay;

        public StreamStoreLinearBackOff(int numberOfRetries, TimeSpan delay)
        {
            if (numberOfRetries < 1)
                throw new ArgumentException($"{nameof(numberOfRetries)} needs to be at least 1");

            if (delay.CompareTo(TimeSpan.Zero) < 0)
                throw new ArgumentException($"{nameof(delay)} cannot be negative");

            _numberOfRetries = numberOfRetries;
            _delay = delay;
        }

        internal override IStreamStoreConnectedProjectionMessageHandler ApplyOn(IStreamStoreConnectedProjectionMessageHandler messageHandler)
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
                    .ExecuteAsync(async ct => await messageHandler.HandleAsync(messages, streamGapStrategy, ct), token);
            }

            return new RetryMessageHandler(ExecuteWithRetryPolicy, projection, messageHandlerLogger);
        }
    }

    internal class KafkaLinearBackOff<TException> : KafkaMessageHandlingRetryPolicy, ILinearBackOff
        where TException : Exception
    {
        private readonly int _numberOfRetries;
        private readonly TimeSpan _delay;

        public KafkaLinearBackOff(int numberOfRetries, TimeSpan delay)
        {
            if (numberOfRetries < 1)
                throw new ArgumentException($"{nameof(numberOfRetries)} needs to be at least 1");

            if (delay.CompareTo(TimeSpan.Zero) < 0)
                throw new ArgumentException($"{nameof(delay)} cannot be negative");

            _numberOfRetries = numberOfRetries;
            _delay = delay;
        }

        internal override IKafkaConnectedProjectionMessageHandler ApplyOn(IKafkaConnectedProjectionMessageHandler messageHandler)
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

            async Task ExecuteWithRetryPolicy(IEnumerable<object> messages, CancellationToken token)
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
                    .ExecuteAsync(async ct => await messageHandler.HandleAsync(messages, ct), token);
            }

            return new RetryMessageHandler(ExecuteWithRetryPolicy, projection, messageHandlerLogger);
        }
    }
}
