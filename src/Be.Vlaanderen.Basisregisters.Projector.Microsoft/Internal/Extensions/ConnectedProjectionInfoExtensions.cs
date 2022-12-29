namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Internal.Extensions
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    internal static class ConnectedProjectionInfoExtensions
    {
        public static string GetProjectionName(this Type type)
            => type.GetAttribute<ConnectedProjectionNameAttribute>() ?? string.Empty;

        public static string GetProjectionDescription(this Type type)
            => type.GetAttribute<ConnectedProjectionDescriptionAttribute>() ?? string.Empty;

        private static T? GetAttribute<T>(this ICustomAttributeProvider type)
            where T : Attribute
            => (T?)type
			  ?.GetCustomAttributes(typeof(T), false)
			  .SingleOrDefault();
    }
}
