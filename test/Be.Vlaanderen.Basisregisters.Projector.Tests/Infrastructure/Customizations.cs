namespace Be.Vlaanderen.Basisregisters.Projector.Tests.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using ConnectedProjections;
    using Internal;
    using Moq;
    using ProjectionHandling.Connector;
    using ProjectionHandling.Runner;
    using TestProjections.OtherProjections;
    using TestProjections.Projections;

    public static class Customizations
    {


        public static void CustomizeRegisteredProjectionsStub(this IFixture fixture)
        {
            fixture.Customize<RegisteredProjections>(composer =>
                    composer.FromFactory(() => new RegisteredProjections(
                        new List<IConnectedProjection>
                        {
                            CreateConnectedProjectionStub<OtherSlowProjections, OtherProjectionContext>(),
                            CreateConnectedProjectionStub<OtherRandomProjections, OtherProjectionContext>(),
                            CreateConnectedProjectionStub<FastProjections, ProjectionContext>(),
                            CreateConnectedProjectionStub<SlowProjections, ProjectionContext>(),
                            CreateConnectedProjectionStub<TrackHandledEventsProjection, ProjectionContext>()
                        })));
        }

        private static IConnectedProjection CreateConnectedProjectionStub<TProjection, TContext>()
            where TContext : RunnerDbContext<TContext>
            where TProjection : ConnectedProjection<TContext>
        {
            var projectionMock = new Mock<IConnectedProjection>();
            projectionMock
                .SetupGet(projection => projection.Name)
                .Returns(new ConnectedProjectionName(typeof(TProjection)));

            projectionMock
                .SetupGet(projection => projection.Instance)
                .Returns(() => throw new Exception("Instance not supported in current setup"));

            return projectionMock.Object;
        }

    }
}
