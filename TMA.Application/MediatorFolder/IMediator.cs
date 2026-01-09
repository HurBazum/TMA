namespace TMA.Application.MediatorFolder;

public interface IMediator
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request);
}