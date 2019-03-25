namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System.Collections.Generic;

    public interface IConnectedProjectionsManager
    {
        /// <summary>
        /// Starts all registered projections
        /// </summary>
        void Start();

        /// <summary>
        /// Starts a specific projection
        /// </summary>
        /// <param name="name">Case insensitive name of the projection to start</param>
        void Start(string name);

        /// <summary>
        /// Stops all running projections
        /// </summary>
        void Stop();

        /// <summary>
        /// Stops a specific running projection
        /// </summary>
        /// <param name="name">Case insensitive name of the projection to stop</param>
        void Stop(string name);

        /// <summary>
        /// Lists all the registered projections
        /// </summary>
        IEnumerable<RegisteredConnectedProjection> GetRegisteredProjections();
    }
}
