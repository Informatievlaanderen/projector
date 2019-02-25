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
    using States;

    public class ConnectedProjectionsManager
    {
        private readonly IEnumerable<IConnectedProjection> _connectedProjections;
        private readonly ConnectedProjectionsCatchUpRunner _catchUpRunner;
        private readonly ConnectedProjectionsSubscriptionRunner _subscriptionRunner;
        private readonly EnvelopeFactory _envelopeFactory;
        private readonly ILoggerFactory _loggerFactory;

        public IEnumerable<IConnectedProjectionStatus> ConnectedProjections
        {
            get
            {
                var registeredProjections = _connectedProjections
                    .Select(registeredProjection => new ConnectedProjectionStatus
                    {
                        Name = registeredProjection.Name,
                        State = ProjectionState.Stopped
                    }).ToList();

                foreach (var projectionStatus in registeredProjections)
                {
                    if (_catchUpRunner.IsCatchingUp(projectionStatus.Name))
                        projectionStatus.State = ProjectionState.CatchingUp;

                    if (_subscriptionRunner.HasSubscription(projectionStatus.Name))
                        projectionStatus.State = ProjectionState.Subscribed;
                }
                return registeredProjections;
            }
        }

        public SubscriptionStreamState SubscriptionStreamStatus => _subscriptionRunner.SubscriptionsStreamStatus;

        internal ConnectedProjectionsManager(
            IEnumerable<IRunnerDbContextMigrationHelper> projectionMigrationHelpers,
            IEnumerable<IConnectedProjectionRegistration> projectionRegistrations,
            ILoggerFactory loggerFactory,
            EnvelopeFactory envelopeFactory,
            IConnectedProjectionEventHandler connectedProjectionEventHandler,
            ConnectedProjectionsCatchUpRunner catchUpRunner,
            ConnectedProjectionsSubscriptionRunner subscriptionRunner)
        {
            _envelopeFactory = envelopeFactory ?? throw new ArgumentNullException(nameof(envelopeFactory));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _catchUpRunner = catchUpRunner ?? throw new ArgumentNullException(nameof(catchUpRunner));
            _subscriptionRunner = subscriptionRunner ?? throw new ArgumentNullException(nameof(subscriptionRunner));

            _connectedProjections = projectionRegistrations?
                                       .Select(registered => registered?.CreateConnectedProjection())
                                       .RemoveNullReferences()
                                   ?? throw new ArgumentNullException(nameof(projectionRegistrations));

            if(null == connectedProjectionEventHandler)
                throw new ArgumentNullException(nameof(connectedProjectionEventHandler));

            connectedProjectionEventHandler
                .RegisterHandleFor<SubscribedProjectionHasThrownAnError>(message => TryRestartSubscriptionsAfterErrorInProjection(message.ProjectionInError));
            
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
            if (null == projection ||
                _subscriptionRunner.HasSubscription(projection.Name) ||
                _catchUpRunner.IsCatchingUp(projection.Name))
                return;

            var handlersProperty = projection.ConnectedProjectionType.GetProperty("Handlers", BindingFlags.Public | BindingFlags.Instance);
            var projectionInstance = ((dynamic)projection).Projection;
            var handlers = handlersProperty?.GetValue(projectionInstance);

            var messageHandlerType = typeof(ConnectedProjectionMessageHandler<>).MakeGenericType(projection.ContextType);
            var messageHandler = Activator.CreateInstance(
                messageHandlerType,
                projection.Name,
                handlers,
                _envelopeFactory,
                _loggerFactory);

            DispatchStartProjection(projection, messageHandler, CancellationToken.None);
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

            _catchUpRunner.Stop(projection.Name);
            _subscriptionRunner.Unsubscribe(projection.Name);
        }

        public void StopAllProjections()
        {
            _catchUpRunner.StopAll();
            _subscriptionRunner.UnsubscribeAll();
        }

        private void TryRestartSubscriptionsAfterErrorInProjection(ConnectedProjectionName faultyProjection)
        {
            var healthyStoppedSubscriptions = _subscriptionRunner
                .UnsubscribeAll()
                .Where(name => false == name.Equals(faultyProjection))
                .Select(name => name.ToString())
                .ToList();

            foreach (var projectionName in healthyStoppedSubscriptions)
                TryStartProjection(projectionName);
        }

        private void DispatchStartProjection(IConnectedProjection connectedProjection, dynamic messageHandler, CancellationToken cancellationToken)
        {
            if (null == connectedProjection || cancellationToken.IsCancellationRequested)
                return;

            TaskRunner.Dispatch(async () =>
            {
                if (false == await _subscriptionRunner.TrySubscribe(connectedProjection, messageHandler, cancellationToken))
                    _catchUpRunner.Start(connectedProjection, messageHandler);
            });
        }

        private IConnectedProjection GetProjection(string name) => _connectedProjections.SingleOrDefault(p => p.Name.Equals(name));
    }
}
