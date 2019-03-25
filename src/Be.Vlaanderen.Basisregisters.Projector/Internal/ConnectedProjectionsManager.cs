namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using Commands;
    using ConnectedProjections;

    internal class ConnectedProjectionsManager : IConnectedProjectionsManager
    {
        private readonly RegisteredProjections _registeredProjections;
        private readonly ConnectedProjectionsCommandBus _commandBus;
        private readonly MigrationHelper _migrationHelper;

        public ConnectedProjectionsManager(
            MigrationHelper migrationHelper,
            RegisteredProjections registeredProjections,
            ConnectedProjectionsCommandBus commandBus,
            ConnectedProjectionsCommandHandler commandHandler)
        {
            _registeredProjections = registeredProjections ?? throw new ArgumentNullException(nameof(registeredProjections));

            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _commandBus.Set(commandHandler ?? throw new ArgumentNullException(nameof(commandHandler)));

            _migrationHelper = migrationHelper ?? throw new ArgumentNullException(nameof(migrationHelper));
            _migrationHelper.RunMigrations();
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
