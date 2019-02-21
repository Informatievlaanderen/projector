namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Internal;
    using Internal.Extensions;
    using Internal.Runners;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;
    using ProjectionHandling.SqlStreamStore;
    using SqlStreamStore;
    using States;

    public class ConnectedProjectionsManager
    {
        private readonly IEnumerable<IConnectedProjection> _connectedProjections;
        private readonly ConnectedProjectionsCatchUpRunner _catchUpRunner;
        private readonly ConnectedProjectionsSubscriptionRunner _subscriptionRunner;
        private readonly EnvelopeFactory _envelopeFactory;
        private readonly ILoggerFactory _loggerFactory;

        public IEnumerable<IConnectedProjectionStatus> ConnectedProjections => _connectedProjections;
        public SubscriptionStreamState SubscriptionStreamStatus => _subscriptionRunner.SubscriptionsStreamStatus;

        internal ConnectedProjectionsManager(
            IEnumerable<IRunnerDbContextMigrationHelper> projectionMigrationHelpers,
            IEnumerable<IConnectedProjectionRegistration> projectionRegistrations,
            IReadonlyStreamStore streamStore,
            ILoggerFactory loggerFactory,
            EnvelopeFactory envelopeFactory,
            ConnectedProjectionsCatchUpRunner catchUpRunner)
        {
            _envelopeFactory = envelopeFactory ?? throw new ArgumentNullException(nameof(envelopeFactory));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            _connectedProjections = projectionRegistrations?
                                       .Select(registered => registered?.CreateConnectedProjection())
                                       .RemoveNullReferences()
                                   ?? throw new ArgumentNullException(nameof(projectionRegistrations));

            _catchUpRunner = new ConnectedProjectionsCatchUpRunner(streamStore, _loggerFactory);
            // ToDo: send message to restart subscriptions, instead of passing the TryRestart directly
            _subscriptionRunner = new ConnectedProjectionsSubscriptionRunner(streamStore, TryRestartSubscriptionsAfterErrorInProjection, _loggerFactory);

            RunMigrations(projectionMigrationHelpers ?? throw new ArgumentNullException(nameof(projectionMigrationHelpers)));
        }

        public void RunMigrations(IEnumerable<IRunnerDbContextMigrationHelper> projectionMigrationHelpers)
        {
            var cancellationToken = CancellationToken.None;
            Task.WaitAll(
                projectionMigrationHelpers
                    .Select(helper => helper.RunMigrationsAsync(cancellationToken))
                    .ToArray(),
                cancellationToken
            );
        }

        public void TryStartProjection(string name)
        {
            var projection = GetProjection(name);
            StartProjection(projection);
        }

        public void StartAllProjections()
        {
            foreach (var connectedProjection in _connectedProjections)
                TryStartProjection(connectedProjection.Name.ToString());
        }

        public void TryStopProjection(string name)
        {
            var projection = GetProjection(name);
            if (null == projection)
                return;

            _catchUpRunner.Stop(projection);
            _subscriptionRunner.Unsubscribe(projection);
        }

        public void StopAllProjections()
        {
            _catchUpRunner.StopAll();
            _subscriptionRunner.UnsubscribeAll();

            foreach (var projection in _connectedProjections)
            {
                if (ProjectionState.CatchingUp == projection?.State ||
                    ProjectionState.Subscribed == projection?.State)
                    projection.Update(ProjectionState.Stopped);
            }
        }

        private void TryRestartSubscriptionsAfterErrorInProjection(ConnectedProjectionName faultyProjection)
        {
            var healthyStoppedSubscriptions = _subscriptionRunner
                .UnsubscribeAll()
                .Where(name => false == name.Equals(faultyProjection))
                .Select(name => name.ToString())
                .ToList();

            foreach (var projection in _connectedProjections)
            {
                if (ProjectionState.Subscribed == projection?.State)
                    projection.Update(ProjectionState.Stopped);
            }

            foreach (var projectionName in healthyStoppedSubscriptions)
                TryStartProjection(projectionName);
        }

        private void StartProjection(IConnectedProjection projection)
        {
            if (null == projection ||
                ProjectionState.Subscribed == projection.State ||
                ProjectionState.CatchingUp == projection.State)
                return;

            var handlersProperty = projection.ConnectedProjectionType.GetProperty("Handlers", BindingFlags.Public | BindingFlags.Instance);
            var connectedProjectionInstance = ((dynamic)projection).CreateInstance();
            var handlers = handlersProperty?.GetValue(connectedProjectionInstance);

            var messageHandlerType = typeof(ConnectedProjectionMessageHandler<>).MakeGenericType(projection.ContextType);
            var messageHandler = Activator.CreateInstance(
                messageHandlerType,
                projection.Name,
                handlers,
                _envelopeFactory,
                _loggerFactory);

            DispatchStartProjection(projection, messageHandler, CancellationToken.None);
        }


        private void DispatchStartProjection(IConnectedProjection connectedProjection, dynamic messageHandler, CancellationToken cancellationToken)
        {
            if (null == connectedProjection || cancellationToken.IsCancellationRequested)
                return;

            TaskRunner.Dispatch(async () =>
            {
                if (false == await _subscriptionRunner.TrySubscribe(connectedProjection, messageHandler, cancellationToken))
                {
                    _catchUpRunner.Start(
                        connectedProjection,
                        messageHandler,
                        // ToDo: Send message to Try-Subscribe projection instead recursive TrySubscribe
                        (Action)(() => DispatchStartProjection(connectedProjection, messageHandler, cancellationToken)));
                }
            });
        }

        private IConnectedProjection GetProjection(string name) => _connectedProjections.SingleOrDefault(p => p.Name.Equals(name));
    }
}
