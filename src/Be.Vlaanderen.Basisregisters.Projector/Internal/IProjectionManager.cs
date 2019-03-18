namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Projector.Commands;

    internal interface IProjectionManager
    {
        void Send<TCommand>()
            where TCommand : ConnectedProjectionCommand, new();

        void Send<TCommand>(TCommand command)
            where TCommand : ConnectedProjectionCommand;

        bool IsProjecting(ConnectedProjectionName projectionName);
        IConnectedProjection GetProjection(ConnectedProjectionName projectionName);
    }
}
