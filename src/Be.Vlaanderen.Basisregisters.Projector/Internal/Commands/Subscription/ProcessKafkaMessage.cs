namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands.Subscription
{
    using System.Threading;

    internal class ProcessKafkaMessage : SubscriptionCommand
    {
        public object Message { get; }
        public CancellationToken CancellationToken { get; }

        public ProcessKafkaMessage(
            object message,
            CancellationToken cancellationToken)
        {
            Message = message;
            CancellationToken = cancellationToken;
        }
    }
}
