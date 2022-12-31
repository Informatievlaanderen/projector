namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Exceptions
{
    using System;
    using System.Collections.Generic;
    using ConnectedProjectionsMicrosoft;

    internal class StreamGapDetectedException : Exception
    {
        public StreamGapDetectedException(IEnumerable<long> missingPositions, ConnectedProjectionIdentifier projection)
            : base($"Stream does not contain messages at positions [{string.Join(',', missingPositions)}] for projection {projection}") { }
    }
}
