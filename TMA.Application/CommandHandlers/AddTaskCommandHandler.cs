using TMA.Application.Commands;
using TMA.Application.MediatorFolder;
using TMA.Domain;
using TMA.Application.Dtos;
using TMA.Application.Others;

namespace TMA.Application.CommandHandlers;

public class AddTaskCommandHandler(ITaskRepository taskRepository) : ICommandHandler<AddTaskCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository = taskRepository;

    public async Task<TaskDto> HandleAsync(AddTaskCommand command)
    {
        var task = TaskEntity.Create(command.Title, command.Deadline, command.Priority);
        
        var t = await _taskRepository.AddTaskAsync(task);

        return Transformer.ToDto(t);
    }
}