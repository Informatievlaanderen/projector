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
                .CustomizeConnectedProjectionIdentifiers();

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
                    _fixture.Create<ConnectedProjectionIdentifier>(),
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

            _commandBusMock.Verify(bus => bus.Queue(It.IsAny<StartAll>()), Times.Once);
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
        public async Task When_starting_a_projection_by_id_then_the_start_command_is_dispatched_with_projection_command()
        {
            const string projectionId = "projection-id";
            new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                    .AddProjectionWithId(projectionId)
                    .Build();

            await _sut.Start(projectionId, CancellationToken.None);

            _commandBusMock
                .Verify(bus =>
                    bus.Queue(It.Is<Start>(start => start.Projection.Equals(projectionId))),
                    Times.Once);
        }

        [Fact]
        public async Task When_starting_a_projection_by_id_then_the_user_desired_state_is_updated()
        {
            const string projectionId = "projection-id";
            var projections = new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                .AddProjectionWithId(projectionId)
                .Build();

            await _sut.Start(projectionId, CancellationToken.None);

            projections.First().Projection.Verify(x => x.UpdateUserDesiredState(UserDesiredState.Started, It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task When_starting_an_unknown_projection_by_id_then_no_start_command_is_dispatched()
        {
            const string projectionId = "non-existing-projection";
            _registeredProjections
                .Setup(projections => projections.Exists(new ConnectedProjectionIdentifier(projectionId)))
                .Returns(false);

            await _sut.Stop(projectionId, CancellationToken.None);

            _commandBusMock.Verify(bus => bus.Queue(It.IsAny<Start>()), Times.Never());
        }

        [Fact]
        public async Task When_starting_an_unknown_projection_by_id_then_no_user_desired_state_is_updated()
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

            _commandBusMock.Verify(bus => bus.Queue(It.IsAny<StopAll>()), Times.Once);
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
        public async Task When_stopping_a_projection_by_id_then_the_stop_projection_command_is_dispatched()
        {
            const string projectionId = "projection-id";
            new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                .AddProjectionWithId(projectionId)
                .Build();

            await _sut.Stop(projectionId, CancellationToken.None);

            _commandBusMock
                .Verify(bus =>
                        bus.Queue(It.Is<Stop>(stop => stop.Projection.Equals(projectionId))),
                    Times.Once);
        }

        [Fact]
        public async Task When_stopping_a_projection_by_id_then_the_user_desired_state_is_updated()
        {
            const string projectionId = "projection-id";
            var projections = new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                .AddProjectionWithId(projectionId)
                .Build();

            await _sut.Stop(projectionId, CancellationToken.None);

            projections.First().Projection
                .Verify(x =>
                        x.UpdateUserDesiredState(UserDesiredState.Stopped, It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task When_stopping_an_unknown_projection_by_id_then_no_stop_command_is_dispatched()
        {
            const string projectionId = "unknown-projection-id";

            new RegisteredProjectionsBuilder(_fixture, _registeredProjections)
                .AddProjectionWithId("some-projection", projection => projection.ShouldResume(true))
                .AddProjectionWithId("another-projection")
                .Build();

            await _sut.Stop(projectionId, CancellationToken.None);

            _commandBusMock.Verify(bus => bus.Queue(It.IsAny<Stop>()), Times.Never());
        }

        [Fact]
        public async Task When_stopping_an_unknown_projection_by_id_then_no_user_desired_state_is_updated()
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
                .AddProjectionWithId("some-projection", projection => projection.ShouldResume(true))
                .AddProjectionWithId("another-projection")
                .Build();

            await _sut.Resume(CancellationToken.None);

            _commandBusMock
                .Verify(bus =>
                        bus.Queue(It.Is<Start>(start => start.Projection.Equals("some-projection"))),
                    Times.Once);

            _commandBusMock
                .Verify(bus =>
                        bus.Queue(It.Is<Start>(start => start.Projection.Equals("another-projection"))),
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
