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
            connectedProjectionEventHandler
                .RegisterHandleFor<CatchUpStarted>(message => UpdateProjectionState(message.Projection, ProjectionState.CatchingUp));
            connectedProjectionEventHandler
                .RegisterHandleFor<CatchUpStopped>(message => UpdateProjectionState(message.Projection ,ProjectionState.Stopped));
            connectedProjectionEventHandler
                .RegisterHandleFor<CatchUpFinished>(message => UpdateProjectionState(message.Projection, ProjectionState.Stopped));
            
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

            foreach (var projection in _connectedProjections.Where(p => ProjectionState.Subscribed == p?.State))
                projection.Update(ProjectionState.Stopped);

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

        private void UpdateProjectionState(ConnectedProjectionName name, ProjectionState state)
        {
            GetProjection(name.ToString())?.Update(state);
        }

        private IConnectedProjection GetProjection(string name) => _connectedProjections.SingleOrDefault(p => p.Name.Equals(name));
    }
}
