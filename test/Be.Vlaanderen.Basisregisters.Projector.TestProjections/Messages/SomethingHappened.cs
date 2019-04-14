namespace Be.Vlaanderen.Basisregisters.Projector.TestProjections.Messages
{
    using System;
    using EventHandling;

    [EventName("SomethingHappened")]
    [EventDescription("Indicates that something happened.")]
    public class SomethingHappened : IEvent
    {
        public string Description { get; set; }
        public DateTime On { get; set; }
    }
}
