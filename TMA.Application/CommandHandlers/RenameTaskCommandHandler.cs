using TMA.Application.Commands;
using TMA.Application.MediatorFolder;
using TMA.Application.Dtos;
using TMA.Application.Others;

namespace TMA.Application.CommandHandlers;

public class RenameTaskCommandHandler(ITaskRepository repository) : ICommandHandler<RenameTaskCommand, TaskDto>
{
    private readonly ITaskRepository _repository = repository;

    public async Task<TaskDto> HandleAsync(RenameTaskCommand command)
    {
        var task = await _repository.GetByIdAsync(command.Id);

        if(task != null)
        {
            task.Rename(command.NewTitle);

            await _repository.UpdateTaskAsync(task);
        }

        return Transformer.ToDto(task);
    }
}