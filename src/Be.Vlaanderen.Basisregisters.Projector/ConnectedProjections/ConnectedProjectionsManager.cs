namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Internal;
    using Internal.Extensions;
    using Internal.Runners;
    using Microsoft.Extensions.Logging;
    using ProjectionHandling.Runner;
    using ProjectionHandling.SqlStreamStore;
    using States;
    using System.Threading.Tasks.Dataflow;
    using Internal.Messages;
    using SqlStreamStore;
    using Messages;
    
    internal interface IConnectedProjectionEventBus
    {
        void Send<TMessage>()
            where TMessage : ConnectedProjectionEvent, new();

        void Send<TMessage>(TMessage message)
            where TMessage : ConnectedProjectionEvent;
    }

    public class ConnectedProjectionsManager : IConnectedProjectionEventBus
    {
        private readonly IEnumerable<IConnectedProjection> _connectedProjections;
        private readonly ConnectedProjectionsCatchUpRunner _catchUpRunner;
        private readonly ConnectedProjectionsSubscriptionRunner _subscriptionRunner;
        private readonly ILogger _logger;

        private readonly ActionBlock<ConnectedProjectionEvent> _mailbox;

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
            IEnumerable<IRunnerDbContextMigrator> projectionMigrationHelpers,
            IEnumerable<IConnectedProjectionRegistration> projectionRegistrations,
            IReadonlyStreamStore streamStore,
            ILoggerFactory loggerFactory,
            EnvelopeFactory envelopeFactory)
        {
            _catchUpRunner = new ConnectedProjectionsCatchUpRunner(streamStore, loggerFactory, this);
            _subscriptionRunner = new ConnectedProjectionsSubscriptionRunner(streamStore, loggerFactory, this);
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionsManager>() ?? throw new ArgumentNullException(nameof(loggerFactory));

            _connectedProjections = projectionRegistrations
                                        ?.Select(registered => registered?.CreateConnectedProjection(envelopeFactory, loggerFactory))
                                       .RemoveNullReferences()
                                   ?? throw new ArgumentNullException(nameof(projectionRegistrations));

            RunMigrations(projectionMigrationHelpers ?? throw new ArgumentNullException(nameof(projectionMigrationHelpers)));

            _mailbox = new ActionBlock<ConnectedProjectionEvent>(Handle);
        }

        private void Handle(ConnectedProjectionEvent message)
        {
            _logger.LogInformation("Handling {Event}: {Message}", message.GetType().Name, message);
            switch (message)
            {
                case StartProjectionRequested startProjectionRequested:
                    StartProjection(startProjectionRequested);
                    break;
                case StartAllProjectionsRequested _:
                    StartAllProjections();
                    break;
                case StopProjectionRequested startProjectionRequested:
                    StopProjection(startProjectionRequested.Projection);
                    break;
                case StopAllProjectionsRequested _:
                    StopAllProjections();
                    break;
                case SubscriptionRequested catchUpFinished:
                    StartSubscription(catchUpFinished.Projection);
                    break;
                case CatchUpRequested catchUpRequested:
                    StartCatchUp(catchUpRequested.Projection);
                    break;
                case CatchUpStopped catchUpStopped:
                    _catchUpRunner.Handle(catchUpStopped);
                    break;
                case SubscriptionsHasThrownAnError subscriptionsHasThrownAnError:
                    TryRestartSubscriptionsAfterErrorInProjection(subscriptionsHasThrownAnError.ProjectionInError);
                    break;
                default:
                    _logger.LogError("No handler defined for {Event}", message.GetType().Name);
                    break;

            }
        }

        public void Send<TEvent>()
            where TEvent : ConnectedProjectionEvent, new()
        {
            Send(new TEvent());
        }

        public void Send<TEvent>(TEvent projectionEvent)
            where TEvent : ConnectedProjectionEvent
        {
            _mailbox.SendAsync(projectionEvent);
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

        private void StartProjection(StartProjectionRequested startProjectionRequested)
        {
            Send(new SubscriptionRequested(startProjectionRequested.Projection));
        }

        private void StartAllProjections()
        {
            if (null == _connectedProjections)
                return;

            foreach (var projection in _connectedProjections)
                Send(new StartProjectionRequested(projection?.Name));
        }

        private void StartSubscription(ConnectedProjectionName projectionName)
        {
            if (_catchUpRunner.IsCatchingUp(projectionName))
                return;

            TaskRunner.Dispatch(() =>
            {
                var projection = GetProjection(projectionName);
                _subscriptionRunner.TrySubscribe(projection, CancellationToken.None);
            });
        }

        private void StartCatchUp(ConnectedProjectionName projectionName)
        {
            if(_subscriptionRunner.HasSubscription(projectionName))
                return;

            var projection = GetProjection(projectionName);
            _catchUpRunner.Start(projection);
        }

        private void StopProjection(ConnectedProjectionName projectionName)
        {
            _catchUpRunner.Stop(projectionName);
            _subscriptionRunner.Unsubscribe(projectionName);
        }

        private void StopAllProjections()
        {
            _catchUpRunner.StopAll();
            _subscriptionRunner.UnsubscribeAll();
        }

        private void TryRestartSubscriptionsAfterErrorInProjection(ConnectedProjectionName faultyProjection)
        {
            var healthySubscriptions = _subscriptionRunner
                .UnsubscribeAll()
                .Where(name => false == name.Equals(faultyProjection));

            foreach (var projection in healthySubscriptions)
                Send(new StartProjectionRequested(projection));
        }

        public ConnectedProjectionName FindRegisteredProjectionFor(string name) =>
            _connectedProjections
                .SingleOrDefault(p => p.Name.Equals(name))
                ?.Name;

        private dynamic GetProjection(ConnectedProjectionName name) =>
            _connectedProjections
                .SingleOrDefault(p => p.Name.Equals(name))
                ?.Instance;
    }
}
