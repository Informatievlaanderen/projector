namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Commands;

    public interface IConnectedProjectionsManager
    {
        Task Send<TCommand>()
            where TCommand : ConnectedProjectionCommand, new();

        Task Send<TCommand>(TCommand command)
            where TCommand : ConnectedProjectionCommand;

        IEnumerable<RegisteredConnectedProjection> GetRegisteredProjections();
        ConnectedProjectionName GetRegisteredProjectionName(string name);
    }
}
