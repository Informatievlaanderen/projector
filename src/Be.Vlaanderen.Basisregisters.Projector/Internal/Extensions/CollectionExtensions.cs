namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class CollectionExtensions
    {
        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> collection) => collection.ToList();

        public static IEnumerable<T> RemoveNullReferences<T>(this IEnumerable<T> collection) => collection.Where(item => item != null);

        public static string ToString<T>(this IEnumerable<T> collection, string separator) => string.Join(separator, collection ?? new List<T>());
    }
}
