namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class CollectionExtensions
    {
        public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> collection) => collection?.ToList();

        public static IEnumerable<T> RemoveNullReferences<T>(this IEnumerable<T> collection) => collection?.Where(item => null != item);
    }
}
