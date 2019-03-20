namespace Be.Vlaanderen.Basisregisters.Projector.Commands
{
    using ConnectedProjections;
    using Internal.Commands.Subscription;

    public class Start : ConnectedProjectionCommand
    {
        public ConnectedProjectionCommand DefaultCommand { get; }
        
        public Start(ConnectedProjectionName projectionName) => DefaultCommand = new Subscribe(projectionName);
    }
}
