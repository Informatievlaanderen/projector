namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands
{
    internal interface IConnectedProjectionsCommandBus
    {
        void Queue<TCommand>()
            where TCommand : ConnectedProjectionCommand, new();
        void Queue<TCommand>(TCommand command)
            where TCommand : ConnectedProjectionCommand;
    }
}
