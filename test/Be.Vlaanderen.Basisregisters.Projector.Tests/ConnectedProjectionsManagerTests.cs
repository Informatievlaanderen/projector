namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using System.Collections.Generic;
    using AutoFixture;
    using ConnectedProjections;
    using FluentAssertions;
    using Infrastructure;
    using Internal;
    using Internal.Commands;
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
        public void When_starting_the_projections_then_the_start_all_command_is_dispatched()
        {
            _sut.Start();

            _commandBusMock.Verify(bus => bus.Queue<StartAll>(), Times.Once);
        }

        [Fact]
        public void When_starting_the_projections_then_the_user_desired_state_is_saved()
        {
            _sut.Start();

            _registeredProjections.Verify(bus => bus, Times.Once);
        }

        [Fact]
        public void When_starting_a_projection_by_name_then_the_start_command_is_dispatched_with_projection_command()
        {
            var projectionName = "projection-name";
            var projection = _fixture.Create<ConnectedProjectionName>();
            _registeredProjections
                .Setup(projections => projections.GetName(projectionName))
                .Returns(projection);

            _sut.Start(projectionName);

            _commandBusMock
                .Verify(bus =>
                    bus.Queue(It.Is<Start>(start => start.ProjectionName.Equals(projection))),
                    Times.Once);
        }

        [Fact]
        public void When_starting_an_unknown_projection_by_name_then_no_start_command_is_dispatched()
        {
            var projectionName = "non-existing-projection";
            _registeredProjections
                .Setup(projections => projections.GetName(projectionName))
                .Returns((ConnectedProjectionName)null);

            _sut.Stop(projectionName);

            _commandBusMock.Verify(bus => bus.Queue(It.IsAny<Start>()), Times.Never());
        }

        [Fact]
        public void When_stopping_the_projections_then_the_stop_all_command_is_dispatched()
        {
            _sut.Stop();

            _commandBusMock.Verify(bus => bus.Queue<StopAll>(), Times.Once);
        }

        [Fact]
        public void When_stopping_a_projection_by_name_then_the_stop_projection_command_is_dispatched()
        {
            var projectionName = "projection-name";
            var projection = _fixture.Create<ConnectedProjectionName>();
            _registeredProjections
                .Setup(projections => projections.GetName(projectionName))
                .Returns(projection);

            _sut.Stop(projectionName);

            _commandBusMock
                .Verify(bus =>
                    bus.Queue(It.Is<Stop>(stop => stop.ProjectionName.Equals(projection))),
                    Times.Once);
        }

        [Fact]
        public void When_stopping_an_unknown_projection_by_name_then_no_stop_command_is_dispatched()
        {
            var projectionName = "non-existing-projection";
            _registeredProjections
                .Setup(projections => projections.GetName(projectionName))
                .Returns((ConnectedProjectionName)null);

            _sut.Stop(projectionName);

            _commandBusMock.Verify(bus => bus.Queue(It.IsAny<Stop>()), Times.Never());
        }
    }
}
