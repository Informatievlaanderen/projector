namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using Internal.RetryPolicies;

    public static class RetryPolicy {

        public static MessageHandlingRetryPolicy NoRetries => new NoRetries();

        public static MessageHandlingRetryPolicy LinearBackoff<TException>(int numberOfRetries, TimeSpan initialWait)
            where TException : Exception
            => new LinearBackOff<TException>(numberOfRetries, initialWait);
    }
}
