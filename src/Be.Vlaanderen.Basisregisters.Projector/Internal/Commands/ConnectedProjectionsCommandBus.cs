namespace Be.Vlaanderen.Basisregisters.Projector.Internal.Commands
{
    using System;
    using System.Threading.Tasks.Dataflow;
    using Projector.Commands;

    internal class ConnectedProjectionsCommandBus : IConnectedProjectionsCommandBus
    {
        private readonly ActionBlock<ConnectedProjectionCommand> _mailbox;
        private ConnectedProjectionsCommandHandler _commandHandler;

        public ConnectedProjectionsCommandBus()
        {
            _mailbox = new ActionBlock<ConnectedProjectionCommand>(
                async command =>
                {
                    if(_commandHandler == null)
                        throw new Exception($"No command handler assigned, {command} was not handled");

                    await _commandHandler.Handle(command);
                });
        }

        public void Set(ConnectedProjectionsCommandHandler commandHandler)
        {
            if (_commandHandler != null)
                throw new Exception("CommandHandler is already assigned");

            _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        }


        public void Queue<TCommand>()
            where TCommand : ConnectedProjectionCommand, new()
            => Queue(new TCommand());

        public void Queue<TCommand>(TCommand command)
            where TCommand : ConnectedProjectionCommand
            => _mailbox.SendAsync(command);
    }
}
