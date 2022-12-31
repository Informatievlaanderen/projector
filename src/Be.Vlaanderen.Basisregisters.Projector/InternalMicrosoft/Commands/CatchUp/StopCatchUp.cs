namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Commands.CatchUp
{
    using System;
    using ConnectedProjectionsMicrosoft;

    internal class StopCatchUp : CatchUpCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }

        public StopCatchUp(ConnectedProjectionIdentifier projection) => Projection = projection ?? throw new ArgumentNullException(nameof(projection));
    }
}
