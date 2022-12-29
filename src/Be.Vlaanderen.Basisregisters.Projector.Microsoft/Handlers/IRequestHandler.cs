namespace Be.Vlaanderen.Basisregisters.Projector.Microsoft.Handlers
{
    using System.Threading.Tasks;

    public interface IRequestHandler<in TRequest, TResponse>
    {
        Task<TResponse> Handle(TRequest request);
    }
}
