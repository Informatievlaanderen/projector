namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Exceptions
{
    using System;
    using System.Collections.Generic;
    using ConnectedProjections;

    internal class StreamGapDetectedException : Exception
    {
        public StreamGapDetectedException(IEnumerable<long> missingPositions, ConnectedProjectionName projectionName)
            : base($"Stream does not contain messages at positions [{string.Join(',', missingPositions)}] for projection {projectionName}") { }
    }
}
