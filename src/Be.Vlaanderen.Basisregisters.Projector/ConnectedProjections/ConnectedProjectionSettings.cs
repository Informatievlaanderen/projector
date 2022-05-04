namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using Internal.Configuration;

    public class StreamStoreConnectedProjectionSettings : IStreamStoreConnectedProjectionSettings
    {
        public int CatchUpPageSize { get; }
        public int CatchUpUpdatePositionMessageInterval { get; }

        public StreamStoreMessageHandlingRetryPolicy RetryPolicy { get; }

        internal StreamStoreConnectedProjectionSettings(
            int catchUpPageSize,
            int catchUpUpdatePositionMessageInterval,
            StreamStoreMessageHandlingRetryPolicy retryPolicy)
        {
            if (catchUpPageSize < 1)
                throw new ArgumentException($"{nameof(catchUpPageSize)} has to be at least 1");

            if (catchUpUpdatePositionMessageInterval < 1)
                throw new ArgumentException($"{nameof(catchUpUpdatePositionMessageInterval)} has to be at least 1");

            if (catchUpUpdatePositionMessageInterval > catchUpPageSize)
                throw new ArgumentException($"{nameof(catchUpUpdatePositionMessageInterval)} cannot be larger than {nameof(catchUpPageSize)} ({catchUpPageSize})");

            CatchUpPageSize = catchUpPageSize;
            CatchUpUpdatePositionMessageInterval = catchUpUpdatePositionMessageInterval;
            RetryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        }
        
        public static StreamStoreConnectedProjectionSettings Default
            => new StreamStoreConnectedProjectionSettingsConfigurator().CreateSettings();

        public static StreamStoreConnectedProjectionSettings Configure(Action<StreamStoreConnectedProjectionSettingsConfigurator> configure)
        {
            var configurator = new StreamStoreConnectedProjectionSettingsConfigurator();
            configure(configurator);
            return configurator.CreateSettings();
        }
    }

    public class KafkaConnectedProjectionSettings : IKafkaConnectedProjectionSettings
    {
        public KafkaMessageHandlingRetryPolicy RetryPolicy { get; }

        internal KafkaConnectedProjectionSettings(KafkaMessageHandlingRetryPolicy retryPolicy)
        {
            RetryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        }

        public static KafkaConnectedProjectionSettings Default
            => new KafkaConnectedProjectionSettingsConfigurator().CreateSettings();

        public static KafkaConnectedProjectionSettings Configure(Action<KafkaConnectedProjectionSettingsConfigurator> configure)
        {
            var configurator = new KafkaConnectedProjectionSettingsConfigurator();
            configure(configurator);
            return configurator.CreateSettings();
        }
    }
}
