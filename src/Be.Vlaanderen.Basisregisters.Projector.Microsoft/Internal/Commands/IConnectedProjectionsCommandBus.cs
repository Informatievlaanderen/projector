namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Internal.Commands
{
    internal interface IConnectedProjectionsCommandBus
    {
        void Queue<TCommand>(TCommand command)
            where TCommand : ConnectedProjectionCommand;
    }
}
