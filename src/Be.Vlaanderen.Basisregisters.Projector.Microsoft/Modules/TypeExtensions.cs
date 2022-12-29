using System;

namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Modules
{
    public static class TypeExtensions
    {
        public static bool IsAssignableTo<T>(this Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            return typeof(T).IsAssignableFrom(type);
        }
    }
}
