namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Extensions
{
    using System;
    using ConnectedProjectionsMicrosoft;
    using InternalMicrosoft;

    internal static class MessageHandlerRetryExtensions
    {
        public static IConnectedProjectionMessageHandler WithPolicy(
            this IConnectedProjectionMessageHandler connectedProjectionMessageHandler,
            MessageHandlingRetryPolicy retryPolicy)
        {
            if (connectedProjectionMessageHandler == null)
                throw new ArgumentNullException(nameof(connectedProjectionMessageHandler));

            if (retryPolicy == null)
                throw new ArgumentNullException(nameof(retryPolicy));

            return retryPolicy.ApplyOn(connectedProjectionMessageHandler);
        }
    }
}
