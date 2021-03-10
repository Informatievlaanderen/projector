namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using System;
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


        public RegisteredProjectionsBuilder AddProjectionWithId(string id)
            => AddProjectionWithId(id, mock => { });

        public RegisteredProjectionsBuilder AddProjectionWithId(string id, Action<ProjectionMock> configure)
        {
            var projectionMock = new ProjectionMock(id);
            configure?.Invoke(projectionMock);
            _connectedProjections.Add(projectionMock);

            return this;
        }

        public RegisteredProjectionsBuilder AddRandomProjections()
        {
            for (var i = 0; i < _fixture.Create<int>(); i++)
            {
                var projectionId = _fixture.Create<ConnectedProjectionIdentifier>();
                _connectedProjections.Add(new ProjectionMock(projectionId));
            }

            return this;
        }

        public List<ProjectionMock> Build()
        {
            foreach (var connectedProjection in _connectedProjections)
            {
                _registeredProjectionsMock
                    .Setup(projections => projections.Exists(connectedProjection.ProjectionId))
                    .Returns(true);

                _registeredProjectionsMock
                    .Setup(projections => projections.GetProjection(connectedProjection.ProjectionId))
                    .Returns(connectedProjection.Projection.Object);

                connectedProjection.Projection
                    .Setup(x => x.UpdateUserDesiredState(It.IsAny<UserDesiredState>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                connectedProjection.Projection
                    .Setup(x => x.Id)
                    .Returns(connectedProjection.ProjectionId);
            }

            _registeredProjectionsMock
                .Setup(registeredProjections => registeredProjections.Projections)
                .Returns(_connectedProjections.Select(projectionMock => projectionMock.Projection.Object));

            return _connectedProjections.ToList();
        }

        internal class ProjectionMock
        {
            private bool _shouldResume;

            public ConnectedProjectionIdentifier ProjectionId { get; }
            public Mock<IConnectedProjection> Projection { get; }

            public ProjectionMock(string projectionId)
                :this(BuildConnectedProjectionIdFor(projectionId))
            { }

            public ProjectionMock(ConnectedProjectionIdentifier projectionId)
            {
                ProjectionId = projectionId;
                Projection = new Mock<IConnectedProjection>();

                Projection
                    .Setup(connectedProjection => connectedProjection.ShouldResume(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => _shouldResume);
            }

            private static ConnectedProjectionIdentifier BuildConnectedProjectionIdFor(string name)
            {
                var assemblyName = new AssemblyName("ProjectionsDynamicAssembly");

                var moduleBuilder = AssemblyBuilder
                    .DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect)
                    .DefineDynamicModule(assemblyName.Name);

                var connectedProjectionType = moduleBuilder.DefineType(name, TypeAttributes.Public);

                return new ConnectedProjectionIdentifier(connectedProjectionType);
            }

            public void ShouldResume(bool shouldResume)
            {
                _shouldResume = shouldResume;
            }
        }

    }
}
