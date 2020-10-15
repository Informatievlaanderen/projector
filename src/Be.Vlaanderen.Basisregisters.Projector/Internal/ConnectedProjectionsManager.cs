namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using ConnectedProjections;
    using Extensions;

    internal class ConnectedProjectionsManager : IConnectedProjectionsManager
    {
        private readonly IRegisteredProjections _registeredProjections;
        private readonly IConnectedProjectionsCommandBus _commandBus;

        public ConnectedProjectionsManager(
            IMigrationHelper migrationHelper,
            IRegisteredProjections registeredProjections,
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

            if (migrationHelper == null)
                throw new ArgumentNullException(nameof(migrationHelper));

            commandBusHandlerConfiguration.Register(commandHandler);

            migrationHelper.RunMigrations();
        }

        public IEnumerable<RegisteredConnectedProjection> GetRegisteredProjections()
            => _registeredProjections.GetStates();

        public bool Exists(string name)
            => _registeredProjections.Exists(new ConnectedProjectionName(name));

        public async Task Start(CancellationToken cancellationToken)
        {
            foreach (var projection in _registeredProjections.Projections)
                await projection.UpdateUserDesiredState(UserDesiredState.Started, cancellationToken);

            _commandBus.Queue<StartAll>();
        }

        public async Task Start(string name, CancellationToken cancellationToken)
        {
            var projectionName = new ConnectedProjectionName(name);

            if (!_registeredProjections.Exists(projectionName))
                return; // throw new ArgumentException("Invalid projection name.", nameof(projectionName));

            await _registeredProjections
                .GetProjection(projectionName)
                .UpdateUserDesiredState(UserDesiredState.Started, cancellationToken);

            _commandBus.Queue(new Start(projectionName));
        }

        public async Task Resume(CancellationToken cancellationToken)
        {
            foreach (var projection in _registeredProjections.Projections)
            {
                if (await projection.ShouldResume(cancellationToken))
                    _commandBus.Queue(new Start(projection.Name));
            }
        }

        public async Task Stop(CancellationToken cancellationToken)
        {
            foreach (var projection in _registeredProjections.Projections)
                await projection.UpdateUserDesiredState(UserDesiredState.Stopped, cancellationToken);

            _commandBus.Queue<StopAll>();
        }

        public async Task Stop(string name, CancellationToken cancellationToken)
        {
            var projectionName = new ConnectedProjectionName(name);

            if (!_registeredProjections.Exists(projectionName))
                return; // throw new ArgumentException("Invalid projection name.", nameof(projectionName));

            await _registeredProjections
                .GetProjection(projectionName)
                .UpdateUserDesiredState(UserDesiredState.Stopped, cancellationToken);

            _commandBus.Queue(new Stop(projectionName));
        }
    }
}
