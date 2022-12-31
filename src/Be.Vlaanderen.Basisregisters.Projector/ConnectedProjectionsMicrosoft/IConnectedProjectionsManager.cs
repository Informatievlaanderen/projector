namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjectionsMicrosoft
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.Microsoft.ProjectionStates;

    public interface IConnectedProjectionsManager
    {
        /// <summary>
        /// Starts all registered projections.
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task Start(CancellationToken cancellationToken);

        /// <summary>
        /// Starts a specific projection.
        /// </summary>
        /// <param name="id">Case insensitive id of the projection to start.</param>
        /// <param name="cancellationToken"></param>
        Task Start(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Stops all running projections.
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task Stop(CancellationToken cancellationToken);

        /// <summary>
        /// Stops a specific running projection.
        /// </summary>
        /// <param name="id">Case insensitive id of the projection to stop.</param>
        /// <param name="cancellationToken"></param>
        Task Stop(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Starts any projections that were previously running.
        /// Does not start projections that have never been started, eg: new projections.
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task Resume(CancellationToken cancellationToken);

        /// <summary>
        /// Lists all the registered projections.
        /// </summary>
        IEnumerable<RegisteredConnectedProjection> GetRegisteredProjections();

        /// <summary>
        /// Checks if a specific projection exists.
        /// </summary>
        /// <param name="id">Case insensitive id of the projection to check.</param>
        bool Exists(string id);

        /// <summary>
        /// Get the projection states.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<ProjectionStateItem>> GetProjectionStates(CancellationToken cancellationToken);
    }
}
