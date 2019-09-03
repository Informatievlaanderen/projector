namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using ConnectedProjections;
    using Internal;
    using Moq;

    internal class RegisteredProjectionsBuilder
    {
        private readonly IFixture _fixture;
        private readonly Mock<IRegisteredProjections> _registeredProjectionsMock;
        private readonly IList<ProjectionMock> _connectedProjections;

        public RegisteredProjectionsBuilder(
            IFixture fixture,
            Mock<IRegisteredProjections> registeredProjectionsMock)
        {
            _fixture = fixture;
            _registeredProjectionsMock = registeredProjectionsMock;
            _connectedProjections = new List<ProjectionMock>();
        }

        public RegisteredProjectionsBuilder AddNamedProjection(string projectionNameString, bool shouldResume = false)
        {
            var aName = new AssemblyName("ProjectionsDynamicAssembly");

            var mb =
                AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.RunAndCollect)
                    .DefineDynamicModule(aName.Name);

            var tb = mb.DefineType(
                projectionNameString,
                TypeAttributes.Public);

            var projectionName = new ConnectedProjectionName(tb);
            var projection = new Mock<IConnectedProjection>();

            projection
                .Setup(connectedProjection => connectedProjection.ShouldResume(It.IsAny<CancellationToken>()))
                .ReturnsAsync(shouldResume);

            _connectedProjections.Add(new ProjectionMock(projectionName, projection));

            return this;
        }

        public RegisteredProjectionsBuilder AddRandomProjections()
        {
            for (int i = 0; i < _fixture.Create<int>(); i++)
            {
                var projectionName = _fixture.Create<ConnectedProjectionName>();
                var projection = new Mock<IConnectedProjection>();

                _connectedProjections.Add(new ProjectionMock(projectionName, projection));
            }

            return this;
        }

        public List<ProjectionMock> Build()
        {
            foreach (var connectedProjection in _connectedProjections)
            {
                _registeredProjectionsMock
                    .Setup(projections => projections.Exists(connectedProjection.ProjectionName))
                    .Returns(true);

                _registeredProjectionsMock
                    .Setup(projections => projections.GetProjection(connectedProjection.ProjectionName))
                    .Returns(connectedProjection.Projection.Object);

                connectedProjection.Projection
                    .Setup(x => x.UpdateUserDesiredState(It.IsAny<UserDesiredState>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                connectedProjection.Projection
                    .Setup(x => x.Name)
                    .Returns(connectedProjection.ProjectionName);
            }

            _registeredProjectionsMock
                .Setup(registeredProjections => registeredProjections.Projections)
                .Returns(_connectedProjections.Select(projectionMock => projectionMock.Projection.Object));

            return _connectedProjections.ToList();
        }

        internal class ProjectionMock
        {
            public ConnectedProjectionName ProjectionName { get; }
            public Mock<IConnectedProjection> Projection { get; }

            public ProjectionMock(ConnectedProjectionName projectionName, Mock<IConnectedProjection> projection)
            {
                ProjectionName = projectionName;
                Projection = projection;
            }
        }

    }
}
