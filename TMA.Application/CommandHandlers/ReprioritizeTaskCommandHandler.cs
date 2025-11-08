using TMA.Application.Commands;
using TMA.Application.Others;
using TMA.Application.Dtos;
using TMA.Application.MediatorFolder;

namespace TMA.Application.CommandHandlers;

public class ReprioritizeTaskCommandHandler(ITaskRepository repository) : ICommandHandler<ReprioritizeTaskCommand, TaskDto>
{
    private readonly ITaskRepository _repository = repository;

    public async Task<TaskDto> HandleAsync(ReprioritizeTaskCommand command)
    {
        var task = await _repository.GetByIdAsync(command.Id);

        if(task != null)
        {
            task.Reprioritize(command.Priority);

            await _repository.UpdateTaskAsync(task);
        }

        return Transformer.ToDto(task);
    }
}