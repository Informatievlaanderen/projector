namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using ConnectedProjections;
    using Extensions;
    using ProjectionHandling.Runner.ProjectionStates;

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

        public bool Exists(string id)
            => _registeredProjections.Exists(new ConnectedProjectionIdentifier(id));

        public async Task Start(CancellationToken cancellationToken)
        {
            foreach (var projection in _registeredProjections.Projections)
            {
                await projection.ClearErrorMessage(cancellationToken).NoContext();
                await projection.UpdateUserDesiredState(UserDesiredState.Started, cancellationToken).NoContext();
            }

            _commandBus.Queue<StartAll>();
        }

        public async Task Start(string id, CancellationToken cancellationToken)
        {
            var projection = _registeredProjections.GetProjection(new ConnectedProjectionIdentifier(id));
            if (projection == null)
                return;

            await projection.UpdateUserDesiredState(UserDesiredState.Started, cancellationToken).NoContext();
            await projection.ClearErrorMessage(cancellationToken).NoContext();

            _commandBus.Queue(new Start(projection.Id));
        }

        public async Task Resume(CancellationToken cancellationToken)
        {
            foreach (var projection in _registeredProjections.Projections)
            {
                if (await projection.ShouldResume(cancellationToken).NoContext())
                {
                    await projection.ClearErrorMessage(cancellationToken).NoContext();
                    _commandBus.Queue(new Start(projection.Id));
                }
            }
        }

        public async Task Stop(CancellationToken cancellationToken)
        {
            foreach (var projection in _registeredProjections.Projections)
                await projection.UpdateUserDesiredState(UserDesiredState.Stopped, cancellationToken).NoContext();

            _commandBus.Queue<StopAll>();
        }

        public async Task Stop(string id, CancellationToken cancellationToken)
        {
            var projection = new ConnectedProjectionIdentifier(id);

            if (!_registeredProjections.Exists(projection))
                return; // throw new ArgumentException("Invalid projection Id.", nameof(projection));

            await _registeredProjections
                .GetProjection(projection)!
                .UpdateUserDesiredState(UserDesiredState.Stopped, cancellationToken)
                .NoContext();

            _commandBus.Queue(new Stop(projection));
        }

        public async Task<IEnumerable<ProjectionStateItem>> GetProjectionStates(CancellationToken cancellationToken)
        {
            var list = new List<ProjectionStateItem>();
            foreach (var registeredProjectionsProjection in _registeredProjections.Projections)
            {
                var projectionState = await registeredProjectionsProjection.GetProjectionState(cancellationToken).NoContext();
                if (projectionState != null)
                    list.Add(projectionState);
            }

            return list;
        }
    }
}
