namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands.Subscription
{
    using System.Threading;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    internal class ProcessStreamEvent : SubscriptionCommand
    {
        public ProcessStreamEvent(
            IAllStreamSubscription subscription,
            StreamMessage message,
            CancellationToken cancellationToken)
        {
            Subscription = subscription;
            Message = message;
            CancellationToken = cancellationToken;
        }

        public IAllStreamSubscription Subscription { get; }
        public StreamMessage Message { get; }
        public CancellationToken CancellationToken { get; }
    }
}
