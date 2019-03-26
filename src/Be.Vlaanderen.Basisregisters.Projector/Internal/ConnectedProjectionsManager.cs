namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using Commands;
    using ConnectedProjections;

    internal class ConnectedProjectionsManager : IConnectedProjectionsManager
    {
        private readonly RegisteredProjections _registeredProjections;
        private readonly IConnectedProjectionsCommandBus _commandBus;

        public ConnectedProjectionsManager(
            IMigrationHelper migrationHelper,
            RegisteredProjections registeredProjections,
            IConnectedProjectionsCommandBus commandBus,
            IConnectedProjectionsCommandBusHandlerConfiguration commandBusHandlerConfiguration,
            IConnectedProjectionsCommandHandler commandHandler)
        {
            _registeredProjections = registeredProjections ?? throw new ArgumentNullException(nameof(registeredProjections));
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));

            if (commandBusHandlerConfiguration == null)
                throw new ArgumentNullException(nameof(commandBusHandlerConfiguration));
            if (commandHandler == null)
                throw new ArgumentNullException(nameof(commandHandler));

            commandBusHandlerConfiguration.Register(commandHandler);
            
            if (migrationHelper == null)
                throw new ArgumentNullException(nameof(migrationHelper));

            migrationHelper.RunMigrations();
        }

        public IEnumerable<RegisteredConnectedProjection> GetRegisteredProjections()
            => _registeredProjections.GetStates();

        public void Start() => _commandBus.Queue<StartAll>();

        public void Start(string name)
        {
            var projectionName = _registeredProjections.GetName(name);
            if (projectionName != null)
                _commandBus.Queue(new Start(projectionName));
        }

        public void Stop() => _commandBus.Queue<StopAll>();

        public void Stop(string name)
        {
            var projectionName = _registeredProjections.GetName(name);
            if (projectionName != null)
                _commandBus.Queue(new Stop(projectionName));
        }
    }
}
