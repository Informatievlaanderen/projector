namespace Be.Vlaanderen.Basisregisters.Projector.Internal
{
    using System.Threading.Tasks;
    using ConnectedProjections;
    using Projector.Commands;

    internal interface IProjectionManager
    {
        Task Send<TCommand>()
            where TCommand : ConnectedProjectionCommand, new();

        Task Send<TCommand>(TCommand command)
            where TCommand : ConnectedProjectionCommand;

        bool IsProjecting(ConnectedProjectionName projectionName);
        IConnectedProjection GetProjection(ConnectedProjectionName projectionName);
    }
}
