namespace TMA.Application.MediatorFolder;

public interface IMediator
{
    Task<TResponse> SendASync<TResponse>(IRequest<TResponse> request);
}