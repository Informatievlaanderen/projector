namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjectionsMicrosoft
{
    using System;
    using InternalMicrosoft.Extensions;
    using InternalMicrosoft.RetryPolicies;
    using Microsoft.Extensions.Configuration;

    public class ConnectedProjectionSettingsConfigurator
    {
        private const int DefaultCatchUpPageSize = 1000;
        private int? _catchUpPageSize;
        private int? _catchUpUpdatePositionMessageInterval;
        private MessageHandlingRetryPolicy? _retryPolicy;

        internal ConnectedProjectionSettingsConfigurator() { }

        public ConnectedProjectionSettingsConfigurator ConfigureCatchUpPageSize(int pageSize)
        {
            _catchUpPageSize = pageSize;
            return this;
        }

        public ConnectedProjectionSettingsConfigurator ConfigureCatchUpUpdatePositionMessageInterval(int messagesInterval)
        {
            _catchUpUpdatePositionMessageInterval = messagesInterval;
            return this;
        }

        public ConnectedProjectionSettingsConfigurator ConfigureLinearBackoff<TException>(
            IConfiguration configuration,
            string policyName)
            where TException : Exception
        {
            _retryPolicy = configuration.Configure(
                (numberOfRetries, initialWait) => new LinearBackOff<TException>(numberOfRetries, initialWait),
                config => config.GetValue<int>("NumberOfRetries"),
                config => TimeSpan.FromSeconds(config.GetValue<int>("DelayInSeconds")),
                policyName);
            return this;
        }

        public ConnectedProjectionSettingsConfigurator ConfigureLinearBackoff<TException>(
            int numberOfRetries,
            TimeSpan initialWait)
            where TException : Exception
        {
            _retryPolicy = new LinearBackOff<TException>(numberOfRetries, initialWait);
            return this;
        }

        internal ConnectedProjectionSettings CreateSettings()
        {
            var defaultCatchUpPageSize = _catchUpPageSize ?? DefaultCatchUpPageSize;
            return new ConnectedProjectionSettings(
                defaultCatchUpPageSize,
                _catchUpUpdatePositionMessageInterval ?? defaultCatchUpPageSize,
                _retryPolicy ?? new NoRetries());
        }
    }
}
