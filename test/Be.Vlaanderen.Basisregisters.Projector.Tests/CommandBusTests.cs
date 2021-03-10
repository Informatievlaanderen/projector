namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using Infrastructure;
    using Infrastructure.Extensions;
    using Internal.Commands;
    using Internal.Extensions;
    using Moq;
    using Xunit;

    public class CommandBusTests
    {
        private readonly ConnectedProjectionsCommandBus _sut;
        private readonly Mock<IConnectedProjectionsCommandHandler> _commandHandlerMock;
        private readonly IFixture _fixture;

        public CommandBusTests()
        {
            _fixture = new Fixture()
                .CustomizeConnectedProjectionIdentifiers()
                .CustomizeConnectedProjectionCommands();

            _commandHandlerMock = new Mock<IConnectedProjectionsCommandHandler>();

            var commandBus = new ConnectedProjectionsCommandBus();
            commandBus.Register(_commandHandlerMock.Object);

            _sut = commandBus;
        }

        [Fact]
        public void When_attempting_to_register_a_null_reference_as_command_handler_then_an_argument_null_exception_is_thrown()
        {
            var sut = new ConnectedProjectionsCommandBus();

            Action act = () => sut.Register(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void When_attempting_to_register_a_command_handler_for_a_second_time_then_an_already_registered_exception_is_thrown()
        {
            Action act = () => _sut.Register(new Mock<IConnectedProjectionsCommandHandler>().Object);

            act.Should()
                .Throw<Exception>()
                .WithMessage("CommandHandler is already assigned");
        }

        [Fact]
        public async Task When_queueing_a_command_then_the_command_gets_dispatched_to_the_command_handler()
        {
            var command = _fixture.Create<ConnectedProjectionCommand>();

            _sut.Queue(_fixture.Create<ConnectedProjectionCommand>());
            _sut.Queue(command);
            _sut.Queue(_fixture.Create<ConnectedProjectionCommand>());

            await Task.Delay(300); // give handler time to catch up with queued commands before asserting
            _commandHandlerMock.Verify(handler => handler.Handle(It.Is<ConnectedProjectionCommand>(projectionCommand => ReferenceEquals(projectionCommand, command))), Times.Once);
        }

        [Fact]
        public async Task When_queueing_a_command_type_then_the_an_instance_of_the_command_gets_dispatched_to_the_command_handler()
        {
            _sut.Queue<Generators.CustomCommand>();

            await Task.Delay(300); // give handler time to catch up with queued commands before asserting
            _commandHandlerMock.Verify(handler => handler.Handle(It.IsAny<Generators.CustomCommand>()), Times.Once);
        }

        [Fact]
        public async Task When_queueing_commands_then_the_commands_get_handled_in_the_given_order()
        {
            var commands = _fixture
                .CustomizeConnectedProjectionIdentifiers()
                .CustomizeConnectedProjectionCommands()
                .CreateMany<ConnectedProjectionCommand>(2,10)
                .ToReadOnlyList();

            var handledCommands = new List<ConnectedProjectionCommand>();
            _commandHandlerMock
                .Setup(handler => handler.Handle(It.IsAny<ConnectedProjectionCommand>()))
                .Returns(async (ConnectedProjectionCommand command) =>
                {
                    await Task.Delay(100);
                    handledCommands.Add(command);
                });

            foreach (var command in commands)
                _sut.Queue(command);

            await Task.Delay(commands.Count * 200); // make sure handler has enough time to handler the queued commands

            handledCommands
                .Should()
                .HaveSameCount(commands)
                .And.ContainInOrder(commands);
        }

        [Fact]
        public async Task When_queueing_commands_then_queue_a_other_command_is_not_blocked_by_handling_the_current_queue()
        {
            var commands = _fixture
                .CustomizeConnectedProjectionIdentifiers()
                .CustomizeConnectedProjectionCommands()
                .CreateMany<ConnectedProjectionCommand>(100,500)
                .ToReadOnlyList();

            var commandsHandled = 0;
            _commandHandlerMock
                .Setup(handler => handler.Handle(It.IsAny<ConnectedProjectionCommand>()))
                .Returns(async (ConnectedProjectionCommand command) =>
                {
                    // slowing handler incrementally down
                    await Task.Delay(50 + (commandsHandled + 5));
                    commandsHandled += 1;
                });

            var commandsQueued = 0;
            foreach (var command in commands)
            {
                _sut.Queue(command);
                commandsQueued += 1;
            }

            await Task.Delay(500); // wait for the fist couple of commands to be handled

            commandsHandled
                .Should()
                .BeGreaterThan(0);

            commandsQueued
                .Should()
                .Be(commands.Count)
                .And.BeGreaterThan(commandsHandled);
        }
    }
}
