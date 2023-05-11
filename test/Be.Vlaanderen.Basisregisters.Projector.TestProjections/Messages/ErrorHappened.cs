namespace Be.Vlaanderen.Basisregisters.Projector.TestProjections.Messages
{
    using System;
    using EventHandling;

    [EventName("ErrorHappened")]
    [EventDescription("Simulate an error while handling event.")]
    public class ErrorHappened : IEvent, IMessage
    {
        public DateTime On { get; set; }
    }
}
