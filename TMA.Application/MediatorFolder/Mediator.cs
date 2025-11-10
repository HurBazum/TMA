using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace TMA.Application.MediatorFolder;


// добавить IRequest<TResponse> над IQuery<TResponse> & ICommand<TResponse>  !!!
public class Mediator(IServiceProvider provider) : IMediator
{
    private readonly IServiceProvider _provider = provider;
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        Type? type = null;

        if(request is ICommand<TResponse> command)
        {
            type = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        }
        else if(request is IQuery<TResponse> query)
        {
            type = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        }
        else
        {
            throw new ArgumentException();
        }

        dynamic handler = _provider.GetRequiredService(type);

        return await handler.HandleAsync((dynamic)request);
    }
}