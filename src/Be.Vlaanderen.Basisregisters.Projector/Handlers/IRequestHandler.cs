namespace Be.Vlaanderen.Basisregisters.Projector.Handlers
{
    using System.Threading.Tasks;

    public interface IRequestHandler<in TRequest, TResponse>
    {
        Task<TResponse> Handle(TRequest request);
    }
}
