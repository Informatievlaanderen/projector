namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;

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
    }
}
