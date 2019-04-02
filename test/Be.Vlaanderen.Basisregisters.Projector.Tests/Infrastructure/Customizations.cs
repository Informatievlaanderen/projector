namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Internal;
    using Moq;

    public static class Customizations
    {
        public static IFixture CustomizeFromGenerators<T>(this IFixture fixture, IReadOnlyList<Func<IFixture, T>> generators)
        {
            if (generators == null || generators.Count == 0)
                throw new ArgumentNullException(nameof(generators));

            Func<IFixture, T> GetGenerator(Random random)
                => generators[random.Next(0, generators.Count - 1)];

            T CreateFromFactory(int value)
                => GetGenerator(new Random(value)).Invoke(fixture);

            fixture.Customize<T>(composer =>
                composer.FromFactory<int>(CreateFromFactory));

            return fixture;
        }

        public static IFixture CustomizeRegisteredProjectionsStub(this IFixture fixture)
        {
            fixture.Customize<RegisteredProjections>(composer =>
                composer.FromFactory(() => new RegisteredProjections(
                    Generators.ProjectionName.Select(generator =>
                    {
                        var projectionMock = new Mock<IConnectedProjection>();
                        projectionMock
                            .SetupGet(projection => projection.Name)
                            .Returns(generator(fixture));

                        projectionMock
                            .SetupGet(projection => projection.Instance)
                            .Returns(() => throw new Exception("Instance not supported in current setup"));

                        return projectionMock.Object;
                    }))));

            return fixture;
        }

        public static IFixture CustomizeConnectedProjectionNames(this IFixture fixture)
        {
            return fixture
                .CustomizeFromGenerators(Generators.ProjectionName);
        }

        public static IFixture CustomizeConnectedProjectionCommands(this IFixture fixture)
        {
            return fixture
                .CustomizeFromGenerators(Generators.CatchUpCommand)
                .CustomizeFromGenerators(Generators.SubscriptionCommand)
                .CustomizeFromGenerators(Generators.ProjectionCommand);
        }
    }
}
