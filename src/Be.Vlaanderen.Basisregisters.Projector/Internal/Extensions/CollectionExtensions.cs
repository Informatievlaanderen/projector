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

        public static IEnumerable<T> RemoveNullReferences<T>(this IEnumerable<T> collection) => collection?.Where(item => item != null);

        public static string ToString<T>(this IEnumerable<T> collection, string separator) => string.Join(separator, collection ?? new List<T>());

        public static IConnectedProjection Get(this IEnumerable<IConnectedProjection> connectedProjections, string projectionName)
            => connectedProjections.Find(projectionName);

        public static IConnectedProjection Get(this IEnumerable<IConnectedProjection> connectedProjections, ConnectedProjectionName projectionName)
            => connectedProjections.Find(projectionName);

        private static IConnectedProjection Find(this IEnumerable<IConnectedProjection> connectedProjections, object projectionName)
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
