namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Configuration
{
    internal interface IStreamGapStrategyConfigurationSettings
    {
        int RetryDelayInSeconds { get;  }
        int StreamBufferInSeconds { get;  }
        int PositionBufferSize { get; }
    }

    internal class StreamGapStrategyConfigurationSettings : IStreamGapStrategyConfigurationSettings
    {
        public int RetryDelayInSeconds { get; set; } = 30;
        public int StreamBufferInSeconds { get; set; } = 300;
        public int PositionBufferSize { get; set; } = 1000;
    }
}
