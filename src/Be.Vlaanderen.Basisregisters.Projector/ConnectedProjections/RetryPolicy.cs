namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using Internal.Extensions;
    using Internal.RetryPolicies;
    using Microsoft.Extensions.Configuration;

    public static class RetryPolicy {

        public static MessageHandlingRetryPolicy NoRetries => new NoRetries();

        public static MessageHandlingRetryPolicy LinearBackoff<TException>(
            int numberOfRetries,
            TimeSpan initialWait)
            where TException : Exception
            => new LinearBackOff<TException>(numberOfRetries, initialWait);

        public static MessageHandlingRetryPolicy ConfigureLinearBackoff<TException>(
            IConfiguration configuration,
            string policyName)
            where TException : Exception
            => configuration.Configure(
                LinearBackoff<TException>,
                config => config.GetValue<int>("NumberOfRetries"),
                config => TimeSpan.FromSeconds(config.GetValue<int>("DelayInSeconds")),
                policyName);
    }
}
