namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using Internal.Extensions;
    using Internal.RetryPolicies;
    using Microsoft.Extensions.Configuration;

    public static class RetryPolicy {

        public static MessageHandlingRetryPolicy NoRetries => new NoRetries();

        public static MessageHandlingRetryPolicy LinearBackoff<TException>(int numberOfRetries, TimeSpan initialWait)
            where TException : Exception
            => new LinearBackOff<TException>(numberOfRetries, initialWait);

        public static MessageHandlingRetryPolicy Configure(
            Func<int, TimeSpan, MessageHandlingRetryPolicy> policyFactory,
            IConfiguration configuration,
            string policyName)
            => configuration.Configure(policyFactory, policyName);
    }
}
