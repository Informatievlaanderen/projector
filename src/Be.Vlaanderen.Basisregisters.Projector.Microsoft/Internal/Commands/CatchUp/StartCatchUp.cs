namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Internal.Commands.CatchUp
{
    using System;
    using ConnectedProjections;

    internal class StartCatchUp : CatchUpCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }

        public StartCatchUp(ConnectedProjectionIdentifier projection) => Projection = projection ?? throw new ArgumentNullException(nameof(projection));
    }
}
