namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Extensions
{
    using System;
    using ConnectedProjections;

    internal static class MessageHandlerRetryExtensions
    {
        public static IStreamStoreConnectedProjectionMessageHandler WithPolicy(
            this IStreamStoreConnectedProjectionMessageHandler connectedProjectionMessageHandler,
            StreamStoreMessageHandlingRetryPolicy retryPolicy)
        {
            if (connectedProjectionMessageHandler == null)
                throw new ArgumentNullException(nameof(connectedProjectionMessageHandler));

            if (retryPolicy == null)
                throw new ArgumentNullException(nameof(retryPolicy));

            return retryPolicy.ApplyOn(connectedProjectionMessageHandler);
        }

        public static IKafkaConnectedProjectionMessageHandler WithPolicy(
            this IKafkaConnectedProjectionMessageHandler connectedProjectionMessageHandler,
            KafkaMessageHandlingRetryPolicy retryPolicy)
        {
            if (connectedProjectionMessageHandler == null)
                throw new ArgumentNullException(nameof(connectedProjectionMessageHandler));

            if (retryPolicy == null)
                throw new ArgumentNullException(nameof(retryPolicy));

            return retryPolicy.ApplyOn(connectedProjectionMessageHandler);
        }
    }
}
