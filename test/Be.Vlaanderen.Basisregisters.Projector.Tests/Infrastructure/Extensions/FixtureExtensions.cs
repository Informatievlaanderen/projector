namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure.Extensions
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using static System.Math;

    public static class FixtureExtensions
    {
        public static IEnumerable<T> CreateMany<T>(this IFixture fixture, int minimum, int maximum)
        {
            if (minimum < 0)
                throw new ArgumentException($"{nameof(minimum)} cannot be less than 0");
            if (minimum > maximum)
                throw new ArgumentException($"{nameof(minimum)}:{minimum} cannot be bigger then {nameof(maximum)}: {maximum}");

            var many = new Random(fixture.Create<int>()).Next(minimum, maximum);
            return fixture.CreateMany<T>(many);
        }

        public static T CreatePositive<T>(this IFixture fixture)
        {
            object GetAbsoluteValue()
            {
                var value = fixture.Create<T>();
                return value switch
                {
                    int i => Abs(i),
                    long l => Abs(l),
                    double d => Abs(d),
                    decimal d => Abs(d),
                    _ => throw new NotImplementedException($"Type {typeof(T)} is not supported for CreatePositive")
                };
            }

            return (T)GetAbsoluteValue();
        }
        public static T CreateNegative<T>(this IFixture fixture)
        {
            object GetAbsoluteValue()
            {
                var value = fixture.Create<T>();
                return value switch
                {
                    int i => - Abs(i),
                    long l => - Abs(l),
                    double d => - Abs(d),
                    decimal d => - Abs(d),
                    _ => throw new NotImplementedException($"Type {typeof(T)} is not supported for CreateNegative")
                };
            }

            return (T)GetAbsoluteValue();
        }
    }
}
