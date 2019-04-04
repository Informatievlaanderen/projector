namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CollectionExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
            => collection?.OrderBy(x => Guid.NewGuid());
    }
}
