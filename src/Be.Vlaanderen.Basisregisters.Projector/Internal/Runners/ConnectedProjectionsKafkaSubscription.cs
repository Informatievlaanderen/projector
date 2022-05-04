namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Runners
{
    using System.Threading.Tasks;

    internal interface IConnectedProjectionsKafkaSubscription
    {
        //bool StreamIsRunning { get; }
        Task<long?> Start();
    }

    internal class ConnectedProjectionsKafkaSubscription : IConnectedProjectionsKafkaSubscription
    {
        public Task<long?> Start()
        {
            throw new System.NotImplementedException();
        }
    }
}
