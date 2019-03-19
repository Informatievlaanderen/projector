namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Commands.CatchUp;
    using Commands.Subscription;
    using ConnectedProjections;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;
    using ProjectionHandling.SqlStreamStore;
    using Projector.Commands;
    using Runners;
    using SqlStreamStore;

    internal class ConnectedProjectionsManager : IConnectedProjectionsManager, IProjectionManager
    {
        private readonly IEnumerable<IConnectedProjection> _registeredProjections;
        private readonly ConnectedProjectionsCatchUpRunner _catchUpRunner;
        private readonly ConnectedProjectionsSubscriptionRunner _subscriptionRunner;
        private readonly ILogger _logger;

        private readonly ActionBlock<ConnectedProjectionCommand> _mailbox;

        internal ConnectedProjectionsManager(
            IEnumerable<IRunnerDbContextMigrator> projectionMigrationHelpers,
            IEnumerable<IConnectedProjectionRegistration> projectionRegistrations,
            IReadonlyStreamStore streamStore,
            ILoggerFactory loggerFactory,
            EnvelopeFactory envelopeFactory)
        {
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionsManager>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            _catchUpRunner = new ConnectedProjectionsCatchUpRunner(streamStore, loggerFactory, this);
            _subscriptionRunner = new ConnectedProjectionsSubscriptionRunner(streamStore, loggerFactory, this);
            _registeredProjections = projectionRegistrations?.RegisterWith(envelopeFactory, loggerFactory) ?? throw new ArgumentNullException(nameof(projectionRegistrations));

            _mailbox = new ActionBlock<ConnectedProjectionCommand>(Handle);

            RunMigrations(projectionMigrationHelpers ?? throw new ArgumentNullException(nameof(projectionMigrationHelpers)));
        }

        public void Send<TCommand>()
            where TCommand : ConnectedProjectionCommand, new()
        {
            Send(new TCommand());
        }

        public void Send<TCommand>(TCommand command)
            where TCommand : ConnectedProjectionCommand
        {
            _mailbox.SendAsync(command);
        }

        public IEnumerable<RegisteredConnectedProjection> GetRegisteredProjections()
        {
            return _registeredProjections
                .Select(projection =>
                    new RegisteredConnectedProjection(
                        projection.Name,
                        GetState(projection.Name)));
        }

        public ConnectedProjectionName GetRegisteredProjectionName(string name)
        {
            return _registeredProjections
                ?.SingleOrDefault(status => status.Name.Equals(name))
                ?.Name;
        }

        public bool IsProjecting(ConnectedProjectionName projectionName)
        {
            return ConnectedProjectionState.Stopped != GetState(projectionName);
        }

        public IConnectedProjection GetProjection(ConnectedProjectionName projectionName)
        {
            return _registeredProjections.Get(projectionName);
        }

        private async Task Handle(ConnectedProjectionCommand command)
        {
            switch (command)
            {
                case SubscriptionCommand subscriptionCommand:
                    await _subscriptionRunner.HandleSubscriptionCommand(subscriptionCommand);
                    return;
                case CatchUpCommand catchUpCommand:
                    _catchUpRunner.HandleCatchUpCommand(catchUpCommand);
                    return;
            }

            _logger.LogTrace("Handling {Command}", command);
            switch (command)
            {
                case Start start:
                    Send(start.DefaultCommand);
                    break;
                case StartAll _:
                    foreach (var projection in _registeredProjections ?? new List<IConnectedProjection>())
                        Send(new Start(projection?.Name));
                    break;
                case Stop stop:
                    Send(new StopCatchUp(stop.ProjectionName));
                    Send(new Unsubscribe(stop.ProjectionName));
                    break;
                case StopAll _:
                    Send<StopAllCatchUps>();
                    Send<UnsubscribeAll>();
                    break;
                default:
                    _logger.LogError("No handler defined for {Command}", command);
                    break;
            }
        }

        private ConnectedProjectionState GetState(ConnectedProjectionName projectionName)
        {
            if (_catchUpRunner.IsCatchingUp(projectionName))
                return ConnectedProjectionState.CatchingUp;

            if (_subscriptionRunner.HasSubscription(projectionName))
                return ConnectedProjectionState.Subscribed;

            return ConnectedProjectionState.Stopped;
        }

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
