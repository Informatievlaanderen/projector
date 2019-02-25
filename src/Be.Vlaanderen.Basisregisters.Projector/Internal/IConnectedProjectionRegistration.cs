namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.SqlStreamStore;

    internal interface IConnectedProjectionRegistration
    {
        IConnectedProjection CreateConnectedProjection(EnvelopeFactory envelopeFactory, ILoggerFactory loggerFactory);
    }
}
