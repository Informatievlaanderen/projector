namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IConnectedProjectionsManager
    {
        /// <summary>
        /// Starts all registered projections
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task Start(CancellationToken cancellationToken);

        /// <summary>
        /// Starts a specific projection
        /// </summary>
        /// <param name="name">Case insensitive name of the projection to start</param>
        /// <param name="cancellationToken"></param>
        Task Start(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Stops all running projections
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task Stop(CancellationToken cancellationToken);

        /// <summary>
        /// Stops a specific running projection
        /// </summary>
        /// <param name="name">Case insensitive name of the projection to stop</param>
        /// <param name="cancellationToken"></param>
        Task Stop(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Lists all the registered projections
        /// </summary>
        IEnumerable<RegisteredConnectedProjection> GetRegisteredProjections();
    }
}
