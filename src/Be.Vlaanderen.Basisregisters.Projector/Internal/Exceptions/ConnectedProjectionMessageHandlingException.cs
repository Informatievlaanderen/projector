namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Exceptions
{
    using System;
    using ConnectedProjections;

    internal class ConnectedProjectionMessageHandlingException : Exception
    {
        public ConnectedProjectionMessageHandlingException(Exception exception, ConnectedProjectionName runnerName, long? runnerPosition)
            : base($"Error occured handling message at position: {runnerPosition}", exception)
        {
            RunnerName = runnerName;
            RunnerPosition = runnerPosition ?? -1L;
        }

        public ConnectedProjectionName RunnerName { get; }
        public long RunnerPosition { get; }
    }
}
