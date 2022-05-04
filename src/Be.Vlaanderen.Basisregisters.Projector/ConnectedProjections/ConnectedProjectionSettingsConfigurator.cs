namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using Internal.Extensions;
    using Internal.RetryPolicies;
    using Microsoft.Extensions.Configuration;

    public class StreamStoreConnectedProjectionSettingsConfigurator
    {
        private const int DefaultCatchUpPageSize = 1000;
        private int? _catchUpPageSize;
        private int? _catchUpUpdatePositionMessageInterval;
        private IHandlingRetryPolicy? _retryPolicy;

        internal StreamStoreConnectedProjectionSettingsConfigurator() { }

        public StreamStoreConnectedProjectionSettingsConfigurator ConfigureCatchUpPageSize(int pageSize)
        {
            _catchUpPageSize = pageSize;
            return this;
        }

        public StreamStoreConnectedProjectionSettingsConfigurator ConfigureCatchUpUpdatePositionMessageInterval(int messagesInterval)
        {
            _catchUpUpdatePositionMessageInterval = messagesInterval;
            return this;
        }

        public StreamStoreConnectedProjectionSettingsConfigurator ConfigureLinearBackOff<TException>(
            IConfiguration configuration,
            string policyName)
            where TException : Exception
        {
            IHandlingRetryPolicy pol = new KafkaNoRetries();
            _retryPolicy = configuration.Configure(
                (numberOfRetries, initialWait) => new StreamStoreLinearBackOff<TException>(numberOfRetries, initialWait),
                config => config.GetValue<int>("NumberOfRetries"),
                config => TimeSpan.FromSeconds(config.GetValue<int>("DelayInSeconds")),
                policyName);
            return this;
        }

        public StreamStoreConnectedProjectionSettingsConfigurator ConfigureLinearBackOff<TException>(
            int numberOfRetries,
            TimeSpan initialWait)
            where TException : Exception
        {
            _retryPolicy = new StreamStoreLinearBackOff<TException>(numberOfRetries, initialWait);
            return this;
        }

        internal StreamStoreConnectedProjectionSettings CreateSettings()
        {
            var defaultCatchUpPageSize = _catchUpPageSize ?? DefaultCatchUpPageSize;
            return new StreamStoreConnectedProjectionSettings(
                defaultCatchUpPageSize,
                _catchUpUpdatePositionMessageInterval ?? defaultCatchUpPageSize,
                (StreamStoreMessageHandlingRetryPolicy?)_retryPolicy ?? new StreamStoreNoRetries());
        }
    }

    public class KafkaConnectedProjectionSettingsConfigurator
    {
        private IHandlingRetryPolicy? _retryPolicy;

        internal KafkaConnectedProjectionSettingsConfigurator() { }

        public KafkaConnectedProjectionSettingsConfigurator ConfigureLinearBackOff<TException>(
            IConfiguration configuration,
            string policyName)
            where TException : Exception
        {
            _retryPolicy = configuration.Configure(
                (numberOfRetries, initialWait) => new KafkaLinearBackOff<TException>(numberOfRetries, initialWait),
                config => config.GetValue<int>("NumberOfRetries"),
                config => TimeSpan.FromSeconds(config.GetValue<int>("DelayInSeconds")),
                policyName);
            return this;
        }

        public KafkaConnectedProjectionSettingsConfigurator ConfigureLinearBackOff<TException>(
            int numberOfRetries,
            TimeSpan initialWait)
            where TException : Exception
        {
            _retryPolicy = new KafkaLinearBackOff<TException>(numberOfRetries, initialWait);
            return this;
        }

        internal KafkaConnectedProjectionSettings CreateSettings()
        {
            return new KafkaConnectedProjectionSettings((KafkaMessageHandlingRetryPolicy?)_retryPolicy ?? new KafkaNoRetries());
        }
    }
}
