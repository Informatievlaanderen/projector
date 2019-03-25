namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands
{
    using ConnectedProjections;
    using Subscription;

    internal class Start : ConnectedProjectionCommand
    {
        public ConnectedProjectionCommand DefaultCommand { get; }
        
        public Start(ConnectedProjectionName projectionName) => DefaultCommand = new Subscribe(projectionName);
    }
}
