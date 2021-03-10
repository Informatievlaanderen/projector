namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using ConnectedProjections;
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

        public static IFixture CustomizeRegisteredProjectionsCollection(this IFixture fixture)
        {
            fixture.Customize<IEnumerable<IConnectedProjection>>(composer =>
                composer.FromFactory(() => Generators.ProjectionIdentifier.Select(generator =>
                {
                    var projectionMock = new Mock<IConnectedProjection>();
                    projectionMock
                        .SetupGet(projection => projection.Id)
                        .Returns(generator(fixture));

                    projectionMock
                        .SetupGet(projection => projection.Info)
                        .Returns(new ConnectedProjectionInfo(string.Empty, string.Empty));

                    projectionMock
                        .SetupGet(projection => projection.Instance)
                        .Returns(() => throw new Exception("Instance not supported in current setup"));

                    return projectionMock.Object;
                })));

            return fixture;
        }

        public static IFixture CustomizeConnectedProjectionIdentifiers(this IFixture fixture)
        {
            return fixture
                .CustomizeFromGenerators(Generators.ProjectionIdentifier);
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
