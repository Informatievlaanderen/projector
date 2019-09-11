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


        public RegisteredProjectionsBuilder AddNamedProjection(string projectionNameString) =>
            AddNamedProjection(projectionNameString, mock => { });

        public RegisteredProjectionsBuilder AddNamedProjection(string projectionNameString, Action<ProjectionMock> configure)
        {
            var projectionMock = new ProjectionMock(projectionNameString);
            configure?.Invoke(projectionMock);
            _connectedProjections.Add(projectionMock);

            return this;
        }

        public RegisteredProjectionsBuilder AddRandomProjections()
        {
            for (var i = 0; i < _fixture.Create<int>(); i++)
            {
                var projectionName = _fixture.Create<ConnectedProjectionName>();
                _connectedProjections.Add(new ProjectionMock(projectionName));
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
            private bool _shouldResume;

            public ConnectedProjectionName ProjectionName { get; }
            public Mock<IConnectedProjection> Projection { get; }

            public ProjectionMock(string projectionName)
                :this(BuildName(projectionName))
            { }

            public ProjectionMock(ConnectedProjectionName projectionName)
            {
                ProjectionName = projectionName;
                Projection = new Mock<IConnectedProjection>();

                Projection
                    .Setup(connectedProjection => connectedProjection.ShouldResume(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => _shouldResume);
            }

            private static ConnectedProjectionName BuildName(string name)
            {
                var assemblyName = new AssemblyName("ProjectionsDynamicAssembly");

                var moduleBuilder = AssemblyBuilder
                    .DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect)
                    .DefineDynamicModule(assemblyName.Name);

                var connectedProjectionType = moduleBuilder.DefineType(name, TypeAttributes.Public);

                return new ConnectedProjectionName(connectedProjectionType);
            }

            public void SetToResume()
            {
                _shouldResume = true;
            }
        }

    }
}
