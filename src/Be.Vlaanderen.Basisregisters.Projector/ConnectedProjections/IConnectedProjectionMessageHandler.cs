namespace Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using SqlStreamStore.Streams;

    public interface IConnectedProjectionMessageHandler
    {
        Task HandleAsync(
            IEnumerable<StreamMessage> messages,
            CancellationToken cancellationToken);
    }
}
