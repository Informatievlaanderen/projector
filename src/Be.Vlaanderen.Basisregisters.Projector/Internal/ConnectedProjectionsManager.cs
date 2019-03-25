namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using ConnectedProjections;
    using ProjectionHandling.Runner;

    internal class ConnectedProjectionsManager : IConnectedProjectionsManager
    {
        private readonly RegisteredProjections _registeredProjections;
        private readonly ConnectedProjectionsCommandBus _commandBus;

        public ConnectedProjectionsManager(
            IEnumerable<IRunnerDbContextMigrator> projectionMigrators,
            RegisteredProjections registeredProjections,
            ConnectedProjectionsCommandBus commandBus,
            ConnectedProjectionsCommandHandler commandHandler)
        {
            _registeredProjections = registeredProjections ?? throw new ArgumentNullException(nameof(registeredProjections));

            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _commandBus.Set(commandHandler ?? throw new ArgumentNullException(nameof(commandHandler)));

            RunMigrations(projectionMigrators ?? throw new ArgumentNullException(nameof(projectionMigrators)));
        }

        public IEnumerable<RegisteredConnectedProjection> GetRegisteredProjections()
            => _registeredProjections.GetStates();

        private void RunMigrations(IEnumerable<IRunnerDbContextMigrator> projectionMigrationHelpers)
        {
            var cancellationToken = CancellationToken.None;
            Task.WaitAll(
                projectionMigrationHelpers
                    .Select(helper => helper.MigrateAsync(cancellationToken))
                    .ToArray(),
                cancellationToken
            );
        }

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
