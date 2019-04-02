namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using System.Linq;
    using AutoFixture;
    using FluentAssertions;
    using Infrastructure;
    using Internal;
    using Internal.Commands;
    using Internal.Commands.Subscription;
    using Moq;
    using Xunit;

    public class ConnectedProjectionsManagerTests
    {
        private readonly ConnectedProjectionsManager _sut;
        private readonly RegisteredProjections _registeredProjections;
        private readonly Mock<IConnectedProjectionsCommandBus> _commandBusMock;

        public ConnectedProjectionsManagerTests()
        {
            var fixture = new Fixture()
                .CustomizeRegisteredProjectionsStub();

            _registeredProjections = fixture.Create<RegisteredProjections>();
            _commandBusMock = new Mock<IConnectedProjectionsCommandBus>();

            _sut = new ConnectedProjectionsManager(
                new Mock<IMigrationHelper>().Object,
                _registeredProjections,
                _commandBusMock.Object,
                new Mock<IConnectedProjectionsCommandBusHandlerConfiguration>().Object,
                new Mock<IConnectedProjectionsCommandHandler>().Object);
        }

        [Fact]
        public void When_requesting_the_registered_projections_then_the_projections_with_state_are_returned()
        {
            _sut.GetRegisteredProjections()
                .Should().BeEquivalentTo(
                    _registeredProjections.GetStates());
        }

        [Fact]
        public void When_starting_the_projections_then_the_start_all_command_is_dispatched()
        {
            _sut.Start();

            _commandBusMock.Verify(bus => bus.Queue<StartAll>(), Times.Once);
        }

        [Fact]
        public void When_starting_a_projection_by_name_then_the_start_command_is_dispatched_with_the_subscribe_projection_as_default_command()
        {
            var projection = _registeredProjections.Names.ToArray()[2];

            _sut.Start(projection.ToString());

            _commandBusMock
                .Verify(bus =>
                    bus.Queue(It.Is<Start>(start => start.DefaultCommand is Subscribe && ((Subscribe)start.DefaultCommand).ProjectionName.Equals(projection))),
                    Times.Once);
        }

        [Fact]
        public void When_starting_a_projection_by_incorrectly_cased_name_then_the_start_command_is_dispatched_with_the_subscribe_projection_as_default_command()
        {
            var projection = _registeredProjections.Names.ToArray()[2];

            _sut.Start(projection.ToString().ToUpper());

            _commandBusMock
                .Verify(bus =>
                    bus.Queue(It.Is<Start>(start => start.DefaultCommand is Subscribe && ((Subscribe)start.DefaultCommand).ProjectionName.Equals(projection))),
                    Times.Once);
        }

        [Fact]
        public void When_starting_an_unknown_projection_by_name_then_no_start_command_is_dispatched()
        {
            _sut.Start("non-existing-projection");

            _commandBusMock.Verify(bus =>bus.Queue(It.IsAny<Start>()), Times.Never());
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
            var projection = _registeredProjections.Names.ToArray()[2];

            _sut.Stop(projection.ToString());

            _commandBusMock
                .Verify(bus =>
                    bus.Queue(It.Is<Stop>(stop => stop.ProjectionName.Equals(projection))),
                    Times.Once);
        }

        [Fact]
        public void When_stopping_a_projection_by_incorrectly_cased_name_then_the_stop_projection_command_is_dispatched()
        {
            var projection = _registeredProjections.Names.ToArray()[2];

            _sut.Stop(projection.ToString().ToUpper());

            _commandBusMock
                .Verify(bus =>
                    bus.Queue(It.Is<Stop>(stop => stop.ProjectionName.Equals(projection))),
                    Times.Once);
        }

        [Fact]
        public void When_stopping_an_unknown_projection_by_name_then_no_stop_command_is_dispatched()
        {
            _sut.Stop("non-existing-projection");

            _commandBusMock.Verify(bus =>bus.Queue(It.IsAny<Stop>()), Times.Never());
        }
    }
}
