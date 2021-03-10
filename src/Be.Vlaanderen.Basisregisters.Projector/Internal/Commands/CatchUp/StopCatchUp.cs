namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands.CatchUp
{
    using System;
    using ConnectedProjections;

    internal class StopCatchUp : CatchUpCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }

        public StopCatchUp(ConnectedProjectionIdentifier projection) => Projection = projection ?? throw new ArgumentNullException(nameof(projection));
    }
}
