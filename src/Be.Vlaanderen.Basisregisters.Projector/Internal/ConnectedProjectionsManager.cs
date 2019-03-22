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
    using Projector.Commands;

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

        public void Send<TCommand>()
            where TCommand : ConnectedProjectionCommand, new()
            => _commandBus.Queue(new TCommand());

        public void Send<TCommand>(TCommand command)
            where TCommand : ConnectedProjectionCommand
            => _commandBus.Queue(command);

        public IEnumerable<RegisteredConnectedProjection> GetRegisteredProjections()
            => _registeredProjections.GetStates();

        public ConnectedProjectionName GetRegisteredProjectionName(string name)
            => _registeredProjections.GetName(name);

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
    }
}
