namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Internal
{
    using System;
    using System.Collections.Generic;
    using SqlStreamStore.Streams;

    internal interface IProcessedStreamState
    {
        public long? LastProcessedMessagePosition { get; }
        public long Position { get; }
        public long ExpectedNextPosition { get; }
        IEnumerable<long> DetermineGapPositions(StreamMessage message);
    }

    internal class ActiveProcessedStreamState : IProcessedStreamState
    {
        private const long NoPosition = -1L;

        private readonly long _lastRunnerPosition;
        
        public long? LastProcessedMessagePosition { get; private set; }
        
        public long Position => Math.Max(_lastRunnerPosition, LastProcessedMessagePosition ?? NoPosition);
        
        public long ExpectedNextPosition => Position + 1;

        public ActiveProcessedStreamState(long? runnerPosition)
            => _lastRunnerPosition = runnerPosition ?? NoPosition;

        public void UpdateWithProcessed(StreamMessage message)
            => LastProcessedMessagePosition = message.Position;

        public IEnumerable<long> DetermineGapPositions(StreamMessage message)
        {
            for (var i = ExpectedNextPosition; i < message.Position; i++)
                yield return i;
        }

        public bool HasChanged => LastProcessedMessagePosition.HasValue && Position != _lastRunnerPosition;
    }
}
