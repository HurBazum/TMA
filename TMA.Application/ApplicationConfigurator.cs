using Microsoft.Extensions.DependencyInjection;
using TMA.Application.Dtos;
using TMA.Application.CommandHandlers;
using TMA.Application.Commands;
using TMA.Application.MediatorFolder;
using TMA.Application.Services;
using TMA.Application.Queries;
using TMA.Application.QueryHandlers;

namespace TMA.Application;

public static class ApplicationConfigurator
{
    public static IServiceCollection ConfigureApplication(this IServiceCollection services) => services
        .AddScoped<IMediator, Mediator>()

        .AddScoped<ICommandHandler<AddTaskCommand, TaskDto>, AddTaskCommandHandler>()
        .AddScoped<ICommandHandler<CompleteTaskCommand, TaskDto>, CompleteTaskCommandHandler>()
        .AddScoped<ICommandHandler<RenameTaskCommand, TaskDto>, RenameTaskCommandHandler>()
        .AddScoped<ICommandHandler<ReprioritizeTaskCommand, TaskDto>, ReprioritizeTaskCommandHandler>()
        .AddScoped<ICommandHandler<RescheduleTaskCommand, TaskDto>, RescheduleTaskCommandHandler>()

        .AddScoped<IQueryHandler<FilterTaskQuery, List<TaskDto>>, FilterTaskQueryHandler>()
        .AddScoped<IQueryHandler<GetByIdQuery, TaskDto>, GetByIdQueryHandler>()
        .AddScoped<IQueryHandler<GetByTitleQuery, TaskDto>, GetByTitleQueryHandler>()
        .AddScoped<IQueryHandler<GetTasksQuery, List<TaskDto>>, GetTasksQueryHandler>()

        .AddScoped<ITaskService<TaskDto>, TaskService>();
}