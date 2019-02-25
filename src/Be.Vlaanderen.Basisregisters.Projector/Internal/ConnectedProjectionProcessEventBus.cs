namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using ConnectedProjections;
    using Microsoft.Extensions.Logging;

    // Poor man's implementation to remove the callback out of manager/subscription/catchup
    // ToDo: replace by lib that does this properly
    internal interface IConnectedProjectionEventBus
    {
        void Send<TMessage>(TMessage message)
            where TMessage : ConnectedProjectionEvent;
    }

    internal interface IConnectedProjectionEventHandler
    {
        void RegisterHandleFor<TMessage>(Action<TMessage> handler)
            where TMessage : ConnectedProjectionEvent;
    }

    internal class ConnectedProjectionProcessEventBus : IConnectedProjectionEventBus, IConnectedProjectionEventHandler
    {
        private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

        private readonly IDictionary<Type, dynamic> _eventHandlers = new Dictionary<Type, dynamic>();
        private readonly ILogger _logger;

        public ConnectedProjectionProcessEventBus(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger<ConnectedProjectionProcessEventBus>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public void RegisterHandleFor<TEvent>(Action<TEvent> handler)
            where TEvent : ConnectedProjectionEvent
        {
            void Handle(TEvent message)
            {
                _logger.LogInformation("Handling {Event}: {Message}", typeof(TEvent), message);
                handler(message);
            }

            var eventType = typeof(TEvent);
            if (_eventHandlers.ContainsKey(eventType))
                _logger.LogError("Already registered handler for {Event}", typeof(TEvent));
            else
                _eventHandlers.Add(eventType, (Action<TEvent>)Handle);
        }

        public void Send<TEvent>(TEvent message)
            where TEvent : ConnectedProjectionEvent
        {
            Lock.Wait(CancellationToken.None);
            try
            {
                var eventType = typeof(TEvent);
                if (_eventHandlers.ContainsKey(eventType))
                    TaskRunner.Dispatch(() => { _eventHandlers[eventType](message); });
                else
                    _logger.LogError("No handler defined for {Event}", eventType);
            }
            finally
            {
                Lock.Release();
            }
        }
    }

    internal abstract class ConnectedProjectionEvent
    {
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

    internal class SubscriptionsHasThrownAnError : ConnectedProjectionEvent
    {
        public ConnectedProjectionName ProjectionInError { get; }

        public SubscriptionsHasThrownAnError(ConnectedProjectionName projectionInError)
        {
            ProjectionInError = projectionInError;
        }
    }

    internal class CatchUpRequested : ConnectedProjectionEvent
    {
        public ConnectedProjectionName Projection { get; }

        public CatchUpRequested(ConnectedProjectionName projection)
        {
            Projection = projection;
        }
    }

    internal class CatchUpStopped : ConnectedProjectionEvent
    {
        public ConnectedProjectionName Projection { get; }

        public CatchUpStopped(ConnectedProjectionName projection)
        {
            Projection = projection;
        }
    }

    internal class CatchUpFinished : ConnectedProjectionEvent
    {
        public ConnectedProjectionName Projection { get; }

        public CatchUpFinished(ConnectedProjectionName projection)
        {
            Projection = projection;
        }
    }

}
