using TMA.Domain;
using TMA.Application.Dtos;
using TMA.Application.Commands;
using TMA.Application.MediatorFolder;
using TMA.Application.Others;
using TMA.Application.Others.Exceptions;

namespace TMA.Application.CommandHandlers;

internal class ExpireTaskCommandHandler(ITaskRepository repository) : ICommandHandler<ExpireTaskCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository = repository;

    public async Task<TaskDto> HandleAsync(ExpireTaskCommand etc)
    {
        TaskEntity task = await _taskRepository.GetByIdAsync(etc.Id)
            ?? throw new TaskWasnotFoundException($"Задача с id={etc.Id.Value} не найдена");

        task.Expire();

        return Transformer.ToDto(task);
    }
}