using TMA.Application.Commands;
using TMA.Domain;
using TMA.Domain.VOs;

namespace TMA.Application.CommandHandlers;

public class AddTaskCommandHandler(ITaskRepository taskRepository)
{
    private readonly ITaskRepository _taskRepository = taskRepository;

    public async Task<TaskId> Handle(AddTaskCommand command)
    {
        var task = TaskEntity.Create(command.TaskTitle, command.Deadline, command.Priority);
        await _taskRepository.AddTaskAsync(task);

        return task.Id;
    }
}