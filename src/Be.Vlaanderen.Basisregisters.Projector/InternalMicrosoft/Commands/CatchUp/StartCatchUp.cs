namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Commands.CatchUp
{
    using System;
    using ConnectedProjectionsMicrosoft;

    internal class StartCatchUp : CatchUpCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }

        public StartCatchUp(ConnectedProjectionIdentifier projection) => Projection = projection ?? throw new ArgumentNullException(nameof(projection));
    }
}
