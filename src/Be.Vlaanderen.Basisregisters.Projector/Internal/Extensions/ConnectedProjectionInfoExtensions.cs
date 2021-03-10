namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Extensions
{
    using System;
    using System.Linq;
    using ProjectionHandling.Connector;

    internal static class ConnectedProjectionInfoExtensions
    {
        public static string GetName(this IConnectedProjection projection)
            => projection.GetAttribute<ConnectedProjectionNameAttribute>() ?? string.Empty;

        public static string GetDescription(this IConnectedProjection projection)
            => projection.GetAttribute<ConnectedProjectionDescriptionAttribute>() ?? string.Empty;

        private static T? GetAttribute<T>(this IConnectedProjection projection)
            where T : Attribute
            => (T?) projection
                ?.GetType()
                ?.GetCustomAttributes(typeof(T), false).SingleOrDefault();
    }
}
