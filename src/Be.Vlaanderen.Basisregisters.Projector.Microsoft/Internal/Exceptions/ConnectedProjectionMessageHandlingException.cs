namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Internal.Exceptions
{
    using System;
    using ConnectedProjections;

    internal class ConnectedProjectionMessageHandlingException : Exception
    {
        public ConnectedProjectionIdentifier Projection { get; }
        public long RunnerPosition { get; }

        public ConnectedProjectionMessageHandlingException(Exception exception, ConnectedProjectionIdentifier projection, IProcessedStreamState? processedState)
            : base($"Error occured handling message at position: {processedState?.LastProcessedMessagePosition}", exception)
        {
            Projection = projection;
            RunnerPosition = processedState?.LastProcessedMessagePosition ?? -1L;
        }
    }
}
