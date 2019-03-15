namespace Be.Vlaanderen.Basisregisters.Projector.Commands
{
    using ConnectedProjections;

    public class Start : ConnectedProjectionCommand
    {
        public ConnectedProjectionCommand DefaultCommand { get; }
        
        public Start(ConnectedProjectionName projectionName)
        {
             DefaultCommand = new Subscription(projectionName);
        }
        
        internal class Subscription : ConnectedProjectionCommand
        {
            public new string Command => $"{typeof(Start).Name}.{GetType().Name}";

            public ConnectedProjectionName ProjectionName { get; }
            
            public Subscription(ConnectedProjectionName projectionName)
            {
                ProjectionName = projectionName;
            }
        }

        internal class CatchUp : ConnectedProjectionCommand
        {
            public new string Command => $"{typeof(Start).Name}.{GetType().Name}";

            public ConnectedProjectionName ProjectionName { get; }
            
            public CatchUp(ConnectedProjectionName projectionName)
            {
                ProjectionName = projectionName;
            }
        }
    }
}
