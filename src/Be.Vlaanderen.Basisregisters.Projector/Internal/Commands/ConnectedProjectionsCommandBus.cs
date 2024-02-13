namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands
{
    using System;
    using System.Threading.Tasks.Dataflow;

    internal interface IConnectedProjectionsCommandBusHandlerConfiguration
    {
        void Register(IConnectedProjectionsCommandHandler commandHandler);
    }

    internal class ConnectedProjectionsCommandBus : IConnectedProjectionsCommandBus, IConnectedProjectionsCommandBusHandlerConfiguration
    {
        private readonly ActionBlock<ConnectedProjectionCommand> _mailbox;
        private IConnectedProjectionsCommandHandler? _commandHandler;

        public ConnectedProjectionsCommandBus()
        {
            _mailbox = new ActionBlock<ConnectedProjectionCommand>(
                async command =>
                {
                    if (_commandHandler == null)
                        throw new Exception($"No command handler assigned, {command} was not handled");

                    await _commandHandler.Handle(command).NoContext();
                });
        }

        public void Register(IConnectedProjectionsCommandHandler commandHandler)
        {
            if (_commandHandler != null)
                throw new Exception("CommandHandler is already assigned");

            _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        }

        public void Queue<TCommand>(TCommand command)
            where TCommand : ConnectedProjectionCommand
            => _mailbox.SendAsync(command).NoContext();
    }
}
