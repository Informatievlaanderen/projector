namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Internal.Commands
{
    using System;
    using ConnectedProjections;

    internal class Restart : ConnectedProjectionCommand
    {
        public ConnectedProjectionIdentifier Projection { get; }
        public TimeSpan After { get; set; }

        public Restart(ConnectedProjectionIdentifier projection, TimeSpan restartAfter)
        {
            Projection = projection ?? throw new ArgumentNullException(nameof(projection));
            After = restartAfter < TimeSpan.Zero ? TimeSpan.Zero : restartAfter;
        }
    }
}
