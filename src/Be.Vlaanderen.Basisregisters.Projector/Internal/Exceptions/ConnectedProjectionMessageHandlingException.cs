namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Exceptions
{
    using System;
    using ConnectedProjections;

    internal class ConnectedProjectionMessageHandlingException : Exception
    {
        public ConnectedProjectionName RunnerName { get; }
        public long RunnerPosition { get; }

        public ConnectedProjectionMessageHandlingException(Exception exception, ConnectedProjectionName runnerName, IProcessedStreamState? processedState)
            : base($"Error occured handling message at position: {processedState?.LastProcessedMessagePosition}", exception)
        {
            RunnerName = runnerName;
            RunnerPosition = processedState?.LastProcessedMessagePosition ?? -1L;
        }
    }
}
