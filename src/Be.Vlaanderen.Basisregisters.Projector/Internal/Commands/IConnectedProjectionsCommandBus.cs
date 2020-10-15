namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands
{
    internal interface IConnectedProjectionsCommandBus
    {
        void Queue<TCommand>(TCommand command)
            where TCommand : ConnectedProjectionCommand;
    }
}
