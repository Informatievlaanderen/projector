namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Commands.CatchUp
{
    using System;
    using ConnectedProjectionsMicrosoft;

    internal class RemoveStoppedCatchUp : CatchUpCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }

        public RemoveStoppedCatchUp(ConnectedProjectionIdentifier projection) => Projection = projection ?? throw new ArgumentNullException(nameof(projection));
    }
}
