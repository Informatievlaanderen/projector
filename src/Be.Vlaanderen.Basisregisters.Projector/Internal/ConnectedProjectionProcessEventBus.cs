namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using ConnectedProjections;

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
        private readonly IDictionary<Type, IList<dynamic>> _messageHandlers = new Dictionary<Type, IList<dynamic>>();
        private readonly Mutex _lock = new Mutex();

        public void RegisterHandleFor<TEvent>(Action<TEvent> handler)
            where TEvent : ConnectedProjectionEvent
        {
            var eventType = typeof(TEvent);
            if (false == _messageHandlers.ContainsKey(eventType))
            {
                _messageHandlers.Add(eventType, new List<dynamic>());
            }

            _messageHandlers[eventType].Add(handler);
        }

        public void Send<TEvent>(TEvent message)
            where TEvent : ConnectedProjectionEvent
        {
            _lock.WaitOne();
            
            var eventType = typeof(TEvent);
            var handlers = _messageHandlers.ContainsKey(eventType)
                ? _messageHandlers[eventType]
                : new List<dynamic>();
            
            foreach (var handler in handlers)
                TaskRunner.Dispatch(() => { handler(message); });
            
            _lock.ReleaseMutex();
        }
    }

    internal class ConnectedProjectionEvent { }

    internal class SubscribedProjectionHasThrownAnError : ConnectedProjectionEvent
    {
        public ConnectedProjectionName ProjectionInError { get; }

        public SubscribedProjectionHasThrownAnError(ConnectedProjectionName projectionInError)
        {
            ProjectionInError = projectionInError;
        }
    }

    internal class CatchUpStarted : ConnectedProjectionEvent
    {
        public ConnectedProjectionName Projection { get; }

        public CatchUpStarted(ConnectedProjectionName projection)
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
