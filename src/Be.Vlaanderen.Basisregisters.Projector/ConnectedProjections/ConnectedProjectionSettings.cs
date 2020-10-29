namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using Internal.Configuration;

    public class ConnectedProjectionSettings : IConnectedProjectionSettings
    {
        internal ConnectedProjectionSettings(int catchUpPageSize, MessageHandlingRetryPolicy retryPolicy)
        {
            if (catchUpPageSize < 1)
                throw new ArgumentException($"{nameof(catchUpPageSize)} has to be at least 1");
            CatchUpPageSize = catchUpPageSize;

            RetryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        }

        public int CatchUpPageSize { get; }
        public MessageHandlingRetryPolicy RetryPolicy { get; }

        public static ConnectedProjectionSettings Default
            => new ConnectedProjectionSettingsConfigurator().CreateSettings();

        public static ConnectedProjectionSettings Configure(Action<ConnectedProjectionSettingsConfigurator> configure)
        {
            var configurator = new ConnectedProjectionSettingsConfigurator();
            configure(configurator);
            return configurator.CreateSettings();
        }
    }
}
