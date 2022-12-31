namespace Be.Vlaanderen.Basisregisters.Projector.InternalMicrosoft.Extensions
{
    using Commands;

    internal static class ConnectedProjectionCommandBusExtensions
    {
        public static void Queue<TCommand>(this IConnectedProjectionsCommandBus commandBus)
            where TCommand : ConnectedProjectionCommand, new()
            => commandBus?.Queue(new TCommand());
    }
}
