namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Infrastructure;
    using Internal.Commands;
    using Internal.Commands.CatchUp;
    using Internal.Commands.Subscription;
    using Internal.Runners;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class ConnectedProjectionsCommandHandlerTests
    {
        private readonly IFixture _fixture;
        private readonly ConnectedProjectionsCommandHandler _sut;
        private readonly FakeLogger _handlerLoggerMock;
        private readonly Mock<IConnectedProjectionsSubscriptionRunner> _subscriptionRunnerMock;
        private readonly Mock<IConnectedProjectionsCatchUpRunner> _catchUpRunnerMock;
        private readonly Mock<IConnectedProjectionsCommandBus> _commandBusMock;

        public ConnectedProjectionsCommandHandlerTests()
        {
            _fixture = new Fixture()
                .CustomizeConnectedProjectionNames()
                .CustomizeConnectedProjectionCommands();
            
            var fakeLoggerFactory = new FakeLoggerFactory();
            _handlerLoggerMock = fakeLoggerFactory.ResolveLoggerMock<ConnectedProjectionsCommandHandler>();
            _subscriptionRunnerMock = new Mock<IConnectedProjectionsSubscriptionRunner>();
            _catchUpRunnerMock = new Mock<IConnectedProjectionsCatchUpRunner>();
            _commandBusMock = new Mock<IConnectedProjectionsCommandBus>();

            _sut = new ConnectedProjectionsCommandHandler(
                _subscriptionRunnerMock.Object,
                _catchUpRunnerMock.Object,
                _commandBusMock.Object,
                fakeLoggerFactory);
        }

        private void VerifyHandleIsLogged<TCommand>(TCommand command, Func<Times> times)
            where TCommand : ConnectedProjectionCommand
            => _handlerLoggerMock.Verify(LogLevel.Trace, $"Handling {command}" ,times);

        [Fact]
        public async Task When_handling_a_catch_up_command_then_the_command_gets_dispatched_to_the_catch_up_runner()
        {
            var command = _fixture.Create<CatchUpCommand>();

            await _sut.Handle(command);

            _catchUpRunnerMock.Verify(runner => runner.HandleCatchUpCommand(command), Times.Once);
        }

        [Fact]
        public async Task When_handling_a_catch_up_command_then_handling_the_command_is_not_logged()
        {
            var command = _fixture.Create<CatchUpCommand>();

            await _sut.Handle(command);

            VerifyHandleIsLogged(command, Times.Never);
        }

        [Fact]
        public async Task When_handling_a_subscription_command_then_the_command_gets_dispatched_to_the_subscription_runner()
        {
            var command = _fixture.Create<SubscriptionCommand>();

            await _sut.Handle(command);

            _subscriptionRunnerMock.Verify(runner => runner.HandleSubscriptionCommand(command), Times.Once);
        }

        [Fact]
        public async Task When_handling_a_subscription_command_then_handling_the_command_is_not_logged()
        {
            var command = _fixture.Create<SubscriptionCommand>();

            await _sut.Handle(command);

            VerifyHandleIsLogged(command, Times.Never);
        }

        [Fact]
        public async Task When_handling_a_general_command_then_handling_the_command_is_logged()
        {

            var command = _fixture.Create<ConnectedProjectionCommand>();
            while (command is CatchUpCommand || command is SubscriptionCommand)
                command = _fixture.Create<ConnectedProjectionCommand>();

            await _sut.Handle(command);

            VerifyHandleIsLogged(command, Times.Once);
        }

        [Fact]
        public async Task When_handling_a_not_defined_general_command_then_handling_error_is_logged()
        {

            var command = new Generators.CustomCommand();

            await _sut.Handle(command);

            _handlerLoggerMock.Verify(LogLevel.Error, $"No handler defined for {command}", Times.Once);
        }

        [Fact]
        public async Task When_handling_a_start_command_then_a_start_subscription_command_is_dispatched()
        {
            var command = _fixture.Create<Start>();

            await _sut.Handle(command);

            _commandBusMock.Verify(
                bus => bus.Queue(It.Is<Subscribe>(subscribe => subscribe.ProjectionName.Equals(command.ProjectionName))),
                Times.Once);
        }        [Fact]

        public async Task When_handling_a_start_all_command_then_a_start_subscribe_all_command_is_dispatched()
        {
            var command = _fixture.Create<StartAll>();

            await _sut.Handle(command);

            _commandBusMock.Verify(
                bus => bus.Queue<SubscribeAll>(),
                Times.Once);
        }

        [Fact]
        public async Task When_handling_a_stop_command_then_an_unsubscribe_command_is_dispatched()
        {
            var command = _fixture.Create<Stop>();

            await _sut.Handle(command);

            _commandBusMock.Verify(
                bus => bus.Queue(It.Is<Unsubscribe>(unsubscribe => unsubscribe.ProjectionName.Equals(command.ProjectionName))),
                Times.Once);
        }

        [Fact]
        public async Task When_handling_a_stop_command_then_a_stop_catch_up_command_is_dispatched()
        {
            var command = _fixture.Create<Stop>();

            await _sut.Handle(command);

            _commandBusMock.Verify(
                bus => bus.Queue(It.Is<StopCatchUp>(stop => stop.ProjectionName.Equals(command.ProjectionName))),
                Times.Once);
        }

        [Fact]
        public async Task When_handling_a_stop_all_command_then_an_unsubscribe_all_command_is_dispatched()
        {
            var command = _fixture.Create<StopAll>();

            await _sut.Handle(command);

            _commandBusMock.Verify(
                bus => bus.Queue<UnsubscribeAll>(),
                Times.Once);
        }

        [Fact]
        public async Task When_handling_a_stop_all_command_then_a_stop_all_catch_ups_command_is_dispatched()
        {
            var command = _fixture.Create<StopAll>();

            await _sut.Handle(command);

            _commandBusMock.Verify(
                bus => bus.Queue<StopAllCatchUps>(),
                Times.Once);
        }
    }
}
