namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System.Collections.Generic;
    using Commands;

    public interface IConnectedProjectionsManager
    {
        void Send<TCommand>()
            where TCommand : ConnectedProjectionCommand, new();

        void Send<TCommand>(TCommand command)
            where TCommand : ConnectedProjectionCommand;

        IEnumerable<RegisteredConnectedProjection> GetRegisteredProjections();
        ConnectedProjectionName GetRegisteredProjectionName(string name);
    }
}
