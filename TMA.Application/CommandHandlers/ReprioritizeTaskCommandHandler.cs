using TMA.Application.Commands;
using TMA.Application.Dtos;
using TMA.Application.MediatorFolder;
using TMA.Application.Others;
using TMA.Application.Others.Exceptions;

namespace TMA.Application.CommandHandlers;

public class ReprioritizeTaskCommandHandler(ITaskRepository repository) : ICommandHandler<ReprioritizeTaskCommand, TaskDto>
{
    private readonly ITaskRepository _repository = repository;

    public async Task<TaskDto> HandleAsync(ReprioritizeTaskCommand command)
    {
        var task = await _repository.GetByIdAsync(command.Id)
            ?? throw new TaskWasnotFoundException($"Задача с id={command.Id.Value} не найдена");

        task.Reprioritize(command.Priority);
        
        task = await _repository.UpdateTaskAsync(task);

        return Transformer.ToDto(task);
    }
}