namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Internal.Commands.CatchUp
{
    using System;
    using ConnectedProjections;

    internal class RemoveStoppedCatchUp : CatchUpCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }

        public RemoveStoppedCatchUp(ConnectedProjectionIdentifier projection) => Projection = projection ?? throw new ArgumentNullException(nameof(projection));
    }
}
