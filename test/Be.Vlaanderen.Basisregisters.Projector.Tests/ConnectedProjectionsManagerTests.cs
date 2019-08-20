namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using ConnectedProjections;
    using FluentAssertions;
    using Infrastructure;
    using Internal;
    using Internal.Commands;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Moq;
    using Xunit;

    public class ConnectedProjectionsManagerTests
    {
        private readonly IFixture _fixture;
        private readonly ConnectedProjectionsManager _sut;
        private readonly Mock<IRegisteredProjections> _registeredProjections;
        private readonly Mock<IConnectedProjectionsCommandBus> _commandBusMock;

        public ConnectedProjectionsManagerTests()
        {
            _fixture = new Fixture()
                .CustomizeConnectedProjectionNames();

            _registeredProjections = new Mock<IRegisteredProjections>();
            _commandBusMock = new Mock<IConnectedProjectionsCommandBus>();

            _sut = new ConnectedProjectionsManager(
                new Mock<IMigrationHelper>().Object,
                _registeredProjections.Object,
                _commandBusMock.Object,
                new Mock<IConnectedProjectionsCommandBusHandlerConfiguration>().Object,
                new Mock<IConnectedProjectionsCommandHandler>().Object);
        }

        [Fact]
        public void When_requesting_the_registered_projections_then_the_projections_with_state_are_returned()
        {
            IEnumerable<RegisteredConnectedProjection> projectionStates = new[]
            {
                new RegisteredConnectedProjection(
                    _fixture.Create<ConnectedProjectionName>(),
                    ConnectedProjectionState.CatchingUp),
            };

            _registeredProjections
                .Setup(projections => projections.GetStates())
                .Returns(projectionStates);

            _sut.GetRegisteredProjections()
                .Should()
                .BeEquivalentTo(projectionStates);
        }

        [Fact]
        public async Task When_starting_the_projections_then_the_start_all_command_is_dispatched()
        {
            await _sut.Start(CancellationToken.None);

            _commandBusMock.Verify(bus => bus.Queue<StartAll>(), Times.Once);
        }

        [Fact]
        public async Task When_starting_the_projections_then_all_user_desired_states_is_updated()
        {
            var projections = SetUpRandomProjections();

            await _sut.Start(CancellationToken.None);

            projections.ForEach(mock => mock.Verify(
                projection =>
                    projection.UpdateUserDesiredState(UserDesiredState.Started, It.IsAny<CancellationToken>()),
                Times.Once));
        }

        [Fact]
        public async Task When_starting_a_projection_by_name_then_the_start_command_is_dispatched_with_projection_command()
        {
            var projectionNameString = "projection-name";
            var projection = SetUpNamedProjection(projectionNameString);

            await _sut.Start(projectionNameString, CancellationToken.None);

            _commandBusMock
                .Verify(bus =>
                    bus.Queue(It.Is<Start>(start => start.ProjectionName.Equals(projection.ProjectionName))),
                    Times.Once);
        }

        [Fact]
        public async Task When_starting_a_projection_by_name_then_the_user_desired_state_is_updated()
        {
            var projectionNameString = "projection-name";
            var projection = SetUpNamedProjection(projectionNameString);

            await _sut.Start(projectionNameString, CancellationToken.None);

            projection.Projection.Verify(x => x.UpdateUserDesiredState(UserDesiredState.Started, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task When_starting_an_unknown_projection_by_name_then_no_start_command_is_dispatched()
        {
            var projectionNameString = "non-existing-projection";
            _registeredProjections
                .Setup(projections => projections.GetName(projectionNameString))
                .Returns((ConnectedProjectionName)null);

            await _sut.Stop(projectionNameString, CancellationToken.None);

            _commandBusMock.Verify(bus => bus.Queue(It.IsAny<Start>()), Times.Never());
        }

        [Fact]
        public async Task When_starting_an_unknown_projection_by_name_then_no_user_desired_state_is_updated()
        {
            var projections = SetUpRandomProjections();

            await _sut.Start("unknown-projection", CancellationToken.None);

            projections.ForEach(mock => mock.Verify(
                projection =>
                    projection.UpdateUserDesiredState(UserDesiredState.Started, It.IsAny<CancellationToken>()),
                Times.Never));
        }

        [Fact]
        public async Task When_stopping_the_projections_then_the_stop_all_command_is_dispatched()
        {
            await _sut.Stop(CancellationToken.None);

            _commandBusMock.Verify(bus => bus.Queue<StopAll>(), Times.Once);
        }

        [Fact]
        public async Task When_stopping_the_projections_then_the_user_desired_state_is_updated()
        {
            var projections = SetUpRandomProjections();

            await _sut.Stop(CancellationToken.None);

            projections.ForEach(mock =>
                mock.Verify(projection =>
                        projection.UpdateUserDesiredState(UserDesiredState.Stopped, It.IsAny<CancellationToken>()),
                    Times.Once));
        }

        [Fact]
        public async Task When_stopping_a_projection_by_name_then_the_stop_projection_command_is_dispatched()
        {
            var projectionNameString = "projection-name";
            var projection = SetUpNamedProjection(projectionNameString);

            await _sut.Stop(projectionNameString, CancellationToken.None);

            _commandBusMock
                .Verify(bus =>
                        bus.Queue(It.Is<Stop>(stop => stop.ProjectionName.Equals(projection.ProjectionName))),
                    Times.Once);
        }

        [Fact]
        public async Task When_stopping_a_projection_by_name_then_the_user_desired_state_is_updated()
        {
            var projectionNameString = "some-projection";
            var projection = SetUpNamedProjection(projectionNameString);

            await _sut.Stop(projectionNameString, CancellationToken.None);

            projection.Projection
                .Verify(x =>
                        x.UpdateUserDesiredState(UserDesiredState.Stopped, It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task When_stopping_an_unknown_projection_by_name_then_no_stop_command_is_dispatched()
        {
            var projectionNameString = "projection-name";

            _registeredProjections
                .Setup(projections => projections.GetName(projectionNameString))
                .Returns((ConnectedProjectionName)null);

            await _sut.Stop(projectionNameString, CancellationToken.None);

            _commandBusMock.Verify(bus => bus.Queue(It.IsAny<Stop>()), Times.Never());
        }

        [Fact]
        public async Task When_stopping_an_unknown_projection_by_name_then_no_user_desired_state_is_updated()
        {
            var projections = SetUpRandomProjections();

            await _sut.Stop("unknown-projection", CancellationToken.None);

            projections.ForEach(mock => mock.Verify(
                projection =>
                    projection.UpdateUserDesiredState(UserDesiredState.Stopped, It.IsAny<CancellationToken>()),
                Times.Never));
        }

        private List<Mock<IConnectedProjection>> SetUpRandomProjections()
        {
            var connectedProjections = new List<Mock<IConnectedProjection>>();

            for (int i = 0; i < _fixture.Create<int>(); i++)
            {
                var projection = new Mock<IConnectedProjection>();

                projection
                    .Setup(x => x.UpdateUserDesiredState(It.IsAny<UserDesiredState>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                connectedProjections.Add(projection);
            }

            _registeredProjections
                .Setup(projections => projections.Projections)
                .Returns(connectedProjections.Select(mock => mock.Object));

            return connectedProjections;
        }

        private ProjectionMock SetUpNamedProjection(string projectionNameString)
        {
            var projectionName = _fixture.Create<ConnectedProjectionName>();
            var projection = new Mock<IConnectedProjection>();

            _registeredProjections
                .Setup(projections => projections.GetName(projectionNameString))
                .Returns(projectionName);

            _registeredProjections
                .Setup(projections => projections.GetProjection(projectionName))
                .Returns(projection.Object);

            return new ProjectionMock(projectionName,
                projection);
        }

        private class ProjectionMock
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
