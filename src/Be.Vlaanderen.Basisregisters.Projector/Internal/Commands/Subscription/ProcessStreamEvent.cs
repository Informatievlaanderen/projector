namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands.Subscription
{
    using System.Threading;
    using SqlStreamStore.Streams;

    internal class ProcessStreamEvent : SubscriptionCommand
    {
        public StreamMessage Message { get; }
        public CancellationToken CancellationToken { get; }

        public ProcessStreamEvent(
            StreamMessage message,
            CancellationToken cancellationToken)
        {
            Message = message;
            CancellationToken = cancellationToken;
        }
    }
}
