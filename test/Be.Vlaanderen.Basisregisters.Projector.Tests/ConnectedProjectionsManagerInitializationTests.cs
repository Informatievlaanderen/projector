namespace Be.Vlaanderen.Basisregisters.Projector.Tests
{
    using Xunit;
    using Internal;
    using Internal.Commands;
    using Moq;

    public class ConnectedProjectionsManagerInitializationTests
    {
        [Fact]
        public void When_initializing_the_manager_then_the_migration_helper_is_called()
        {
            var migrationHelperMock = new Mock<IMigrationHelper>();
            var projections = new Mock<IRegisteredProjections>().Object;
            var bus = new Mock<IConnectedProjectionsCommandBus>().Object;
            var commandBusHandlerConfiguration = new Mock<IConnectedProjectionsCommandBusHandlerConfiguration>().Object;
            var commandHandler = new Mock<IConnectedProjectionsCommandHandler>().Object;

            var manager = new ConnectedProjectionsManager(
                migrationHelperMock.Object,
                projections,
                bus,
                commandBusHandlerConfiguration,
                commandHandler);

            migrationHelperMock.Verify(helper => helper.RunMigrations(), Times.Once);
        }

        [Fact]
        public void When_initializing_the_manager_Then_the_command_handler_is_assigned_to_the_command_bus()
        {
            var migrationHelper = new Mock<IMigrationHelper>().Object;
            var registeredProjections = new Mock<IRegisteredProjections>().Object;
            var commandBus = new Mock<IConnectedProjectionsCommandBus>().Object;
            var commandBusHandlerConfigurationMock = new Mock<IConnectedProjectionsCommandBusHandlerConfiguration>();
            var commandHandler = new Mock<IConnectedProjectionsCommandHandler>().Object;

            var manager = new ConnectedProjectionsManager(
                migrationHelper,
                registeredProjections,
                commandBus,
                commandBusHandlerConfigurationMock.Object,
                commandHandler);

            commandBusHandlerConfigurationMock.Verify(bus => bus.Register(commandHandler), Times.Once);
        }
    }
}
