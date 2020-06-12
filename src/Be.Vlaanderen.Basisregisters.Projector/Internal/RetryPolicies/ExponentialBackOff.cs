namespace Be.Vlaanderen.Basisregisters.Projector.Internal.RetryPolicies
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Polly;
    using SqlStreamStore.Streams;

    internal class ExponentialBackOff<TException> : MessageHandlingRetryPolicy where TException : Exception
    {
        private readonly int _numberOfRetries;
        private readonly TimeSpan _wait;

        public ExponentialBackOff(int numberOfRetries, TimeSpan wait)
        {
            if (numberOfRetries < 1)
                throw new ArgumentException($"{nameof(numberOfRetries)} needs to be at least 1");
            _numberOfRetries = numberOfRetries;
            _wait = wait;
        }

        public override IConnectedProjectionMessageHandler ApplyOn(IConnectedProjectionMessageHandler messageHandler)
        {
            async Task ExecuteAndRetry(IEnumerable<StreamMessage> messages, CancellationToken token)
            {
                await Policy
                    .Handle<TException>()
                    .WaitAndRetryAsync(_numberOfRetries, CalculateWaitTime)
                    .ExecuteAsync(async ct => await messageHandler.HandleAsync(messages, ct), token);
            }

            return  CreateMessageHandlerFor(ExecuteAndRetry);
        }

        private TimeSpan CalculateWaitTime(int attempt)
            => TimeSpan.FromMilliseconds(attempt * _wait.Milliseconds);
    }
}
