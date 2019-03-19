namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using ConnectedProjections;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.SqlStreamStore;

    internal static class CollectionExtensions
    {
        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> collection) => collection?.ToList();

        public static IEnumerable<T> RemoveNullReferences<T>(this IEnumerable<T> collection) => collection?.Where(item => null != item);

        public static string ToString<T>(this IEnumerable<T> collection, string separator) => string.Join(separator, collection ?? new List<T>());

        public static IConnectedProjection Get(this IEnumerable<IConnectedProjection> connectedProjections, ConnectedProjectionName projectionName)
            => connectedProjections?.SingleOrDefault(projection => projection.Name.Equals(projectionName));

        public static IEnumerable<IConnectedProjection> RegisterWith(
            this IEnumerable<IConnectedProjectionRegistration> registeredProjections,
            EnvelopeFactory envelopeFactory,
            ILoggerFactory loggerFactory)
        {
            IConnectedProjection RegisterProjection(IConnectedProjectionRegistration registered) => registered?.CreateConnectedProjection(envelopeFactory, loggerFactory);

            return registeredProjections
                ?.Select(RegisterProjection)
                .RemoveNullReferences();
        }
    }
}
