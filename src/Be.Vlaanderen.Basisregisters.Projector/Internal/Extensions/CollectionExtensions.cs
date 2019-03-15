namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConnectedProjections;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.SqlStreamStore;

    internal static class CollectionExtensions
    {
        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> collection)
        {
            return collection?.ToList();
        }

        public static IEnumerable<T> RemoveNullReferences<T>(this IEnumerable<T> collection)
        {
            return collection?.Where(item => null != item);
        }

        public static string ToString<T>(this IEnumerable<T> collection, string separator)
        {
            return string.Join(separator, collection ?? new List<T>());
        }

        public static IConnectedProjection Get(this IEnumerable<IConnectedProjection> connectedProjections, ConnectedProjectionName projectionName)
        {
            if (null == connectedProjections || null == projectionName)
                return null;

            return connectedProjections.SingleOrDefault(projection => projection.Name.Equals(projectionName));
        }

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
