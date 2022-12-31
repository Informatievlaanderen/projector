namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Exceptions
{
    using System;
    using ConnectedProjectionsMicrosoft;

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
