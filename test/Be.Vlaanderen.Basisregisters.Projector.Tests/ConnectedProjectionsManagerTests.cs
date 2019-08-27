namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using System;
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
            var projections =
                new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                    .AddRandomProjections()
                    .Build();

            await _sut.Start(CancellationToken.None);

            projections.ForEach(projectionMock => projectionMock.Projection.Verify(
                projection =>
                    projection.UpdateUserDesiredState(UserDesiredState.Started, It.IsAny<CancellationToken>()),
                Times.Once));
        }

        [Fact]
        public async Task When_starting_a_projection_by_name_then_the_start_command_is_dispatched_with_projection_command()
        {
            var projectionNameString = "projection-name";
            new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                    .AddNamedProjection(projectionNameString)
                    .Build();

            await _sut.Start(projectionNameString, CancellationToken.None);

            _commandBusMock
                .Verify(bus =>
                    bus.Queue(It.Is<Start>(start => start.ProjectionName.Equals(projectionNameString))),
                    Times.Once);
        }

        [Fact]
        public async Task When_starting_a_projection_by_name_then_the_user_desired_state_is_updated()
        {
            var projectionNameString = "projection-name";
            var projections = new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                .AddNamedProjection(projectionNameString)
                .Build();

            await _sut.Start(projectionNameString, CancellationToken.None);

            projections.First().Projection.Verify(x => x.UpdateUserDesiredState(UserDesiredState.Started, It.IsAny<CancellationToken>()));
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
            var projections = new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                .AddRandomProjections()
                .Build();

            await _sut.Start("unknown-projection", CancellationToken.None);

            projections.ForEach(projectionMock => projectionMock.Projection.Verify(
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
            var projections = new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                .AddRandomProjections()
                .Build();

            await _sut.Stop(CancellationToken.None);

            projections.ForEach(projectionMock =>
                projectionMock.Projection.Verify(projection =>
                        projection.UpdateUserDesiredState(UserDesiredState.Stopped, It.IsAny<CancellationToken>()),
                    Times.Once));
        }

        [Fact]
        public async Task When_stopping_a_projection_by_name_then_the_stop_projection_command_is_dispatched()
        {
            var projectionNameString = "projection-name";
            new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                .AddNamedProjection(projectionNameString)
                .Build();

            await _sut.Stop(projectionNameString, CancellationToken.None);

            _commandBusMock
                .Verify(bus =>
                        bus.Queue(It.Is<Stop>(stop => stop.ProjectionName.Equals(projectionNameString))),
                    Times.Once);
        }

        [Fact]
        public async Task When_stopping_a_projection_by_name_then_the_user_desired_state_is_updated()
        {
            var projectionNameString = "projection-name";
            var projections = new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                .AddNamedProjection(projectionNameString)
                .Build();

            await _sut.Stop(projectionNameString, CancellationToken.None);

            projections.First().Projection
                .Verify(x =>
                        x.UpdateUserDesiredState(UserDesiredState.Stopped, It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task When_stopping_an_unknown_projection_by_name_then_no_stop_command_is_dispatched()
        {
            var projectionNameString = "unknown-projection-name";

            new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                .AddNamedProjection("some-projection", true)
                .AddNamedProjection("another-projection")
                .Build();

            await _sut.Stop(projectionNameString, CancellationToken.None);

            _commandBusMock.Verify(bus => bus.Queue(It.IsAny<Stop>()), Times.Never());
        }

        [Fact]
        public async Task When_stopping_an_unknown_projection_by_name_then_no_user_desired_state_is_updated()
        {
            var projections = new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                .AddRandomProjections()
                .Build();

            await _sut.Stop("unknown-projection", CancellationToken.None);

            projections.ForEach(projectionMock => projectionMock.Projection.Verify(
                projection =>
                    projection.UpdateUserDesiredState(UserDesiredState.Stopped, It.IsAny<CancellationToken>()),
                Times.Never));
        }


        [Fact]
        public async Task When_resuming_the_projections_then_the_start_command_is_dispatched_for_projections_with_a_desired_state_started()
        {
            new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                .AddNamedProjection("some-projection", true)
                .AddNamedProjection("another-projection")
                .Build();

            await _sut.Resume(CancellationToken.None);

            _commandBusMock
                .Verify(bus =>
                        bus.Queue(It.Is<Start>(start => start.ProjectionName.Equals("some-projection"))),
                    Times.Once);

            _commandBusMock
                .Verify(bus =>
                        bus.Queue(It.Is<Start>(start => start.ProjectionName.Equals("another-projection"))),
                    Times.Never);
        }

        [Fact]
        public async Task When_resuming_the_projections_then_no_user_desired_state_is_updated()
        {
            var projections = new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                .AddRandomProjections()
                .Build();

            await _sut.Resume(CancellationToken.None);

            projections.ForEach(projectionMock => projectionMock.Projection.Verify(
                projection =>
                    projection.UpdateUserDesiredState(It.IsAny<UserDesiredState>(), It.IsAny<CancellationToken>()),
                Times.Never));
        }

    }
}
