namespace Be.Vlaanderen.Basisregisters.Projector.TestProjections.Projections
{
    using System;
    using System.Threading.Tasks;
    using EventHandling;
    using Messages;
    using ProjectionHandling.Connector;
    using ProjectionHandling.SqlStreamStore;

    public class TrackHandledEventsProjection : ConnectedProjection<ProjectionContext>
    {
        private event Action OnMessageHandled;

        public TrackHandledEventsProjection()
            : this(onMessageHandled: () => { })
        { }

        public TrackHandledEventsProjection(Action onMessageHandled)
        {
            if (onMessageHandled != null)
            {
                OnMessageHandled += onMessageHandled;
            }

            When<Envelope<DelayWasScheduled>>(Handle);
            When<Envelope<SomethingHappened>>(Handle);
            When<Envelope<ErrorHappened>>(Handle);
        }

        private async Task Handle<T>(ProjectionContext context, Envelope<T> envelope)
            where T : IEvent, IMessage
        {
            switch (envelope.Message)
            {
                case DelayWasScheduled _:
                    await Task.Delay(5000);
                    break;
                case ErrorHappened _:
                    await context.AddAsync(new ProcessedEvent
                    {
                        Id = Guid.NewGuid(),
                        Event = typeof(T).Name,
                        Position = envelope.Position,
                        EventTime = envelope.Message.On
                    });
                    OnMessageHandled?.Invoke();
                    throw new Exception("Something went wrong.");
            }

            await context.AddAsync(new ProcessedEvent
            {
                Id = Guid.NewGuid(),
                Event = typeof(T).Name,
                Position = envelope.Position,
                EventTime = envelope.Message.On
            });

            OnMessageHandled?.Invoke();
        }
    }
}
