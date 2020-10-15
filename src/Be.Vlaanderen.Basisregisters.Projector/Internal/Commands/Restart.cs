namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands
{
    using System;
    using ConnectedProjections;

    internal class Restart : ConnectedProjectionCommand
    {
        public ConnectedProjectionName ProjectionName { get; }
        public TimeSpan After { get; set; }

        public Restart(ConnectedProjectionName projectionName, TimeSpan restartAfter)
        {
            ProjectionName = projectionName ?? throw new ArgumentNullException(nameof(projectionName));
            After = restartAfter < TimeSpan.Zero ? TimeSpan.Zero : restartAfter;
        }
    }
}
