namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using Internal.RetryPolicies;
    using Microsoft.Extensions.Configuration;

    [Obsolete("Use ConnectedProjectionSettings to define MessageHandlingRetryPolicy", true)]
    public static class RetryPolicy {

        [Obsolete("Use ConnectedProjectionSettings to define MessageHandlingRetryPolicy", true)]
        public static MessageHandlingRetryPolicy NoRetries => new NoRetries();

        [Obsolete("Use ConnectedProjectionSettings to define MessageHandlingRetryPolicy", true)]
        public static MessageHandlingRetryPolicy LinearBackoff<TException>(
            int numberOfRetries,
            TimeSpan initialWait)
            where TException : Exception
            => ConnectedProjectionSettings
                .Configure(configurator => configurator.ConfigureLinearBackoff<TException>(numberOfRetries, initialWait))
                .RetryPolicy;

        [Obsolete("Use ConnectedProjectionSettings to define MessageHandlingRetryPolicy", true)]
        public static MessageHandlingRetryPolicy ConfigureLinearBackoff<TException>(
            IConfiguration configuration,
            string policyName)
            where TException : Exception
            => ConnectedProjectionSettings
                .Configure(configurator => configurator.ConfigureLinearBackoff<TException>(configuration, policyName))
                .RetryPolicy;
    }
}
