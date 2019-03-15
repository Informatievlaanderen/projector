namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands.CatchUp
{
    internal class StartCatchUp : CatchUpCommand
    {
        public IConnectedProjection Projection { get; }

        public StartCatchUp(IConnectedProjection projection)
        {
            Projection = projection;
        }
    }
}
