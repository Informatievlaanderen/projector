namespace Be.Vlaanderen.Basisregisters.Projector.TestProjections.Messages
{
    using System;
    using EventHandling;

    [EventName("DelayWasScheduled")]
    [EventDescription("A delay in the events was scheduled.")]
    public class DelayWasScheduled : IEvent
    {
        public DateTime On { get; set; }
    }
}
