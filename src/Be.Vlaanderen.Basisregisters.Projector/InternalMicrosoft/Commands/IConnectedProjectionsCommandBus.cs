namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Commands
{
    internal interface IConnectedProjectionsCommandBus
    {
        void Queue<TCommand>(TCommand command)
            where TCommand : ConnectedProjectionCommand;
    }
}
