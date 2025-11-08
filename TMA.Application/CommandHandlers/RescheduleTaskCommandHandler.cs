using TMA.Application.Commands;
using TMA.Application.Dtos;
using TMA.Application.MediatorFolder;
using TMA.Application.Others;

namespace TMA.Application.CommandHandlers
{
    public class RescheduleTaskCommandHandler(ITaskRepository repository) : ICommandHandler<RescheduleTaskCommand, TaskDto>
    {
        private readonly ITaskRepository _repository = repository;

        public async Task<TaskDto> HandleAsync(RescheduleTaskCommand command) 
        {
            var task = await _repository.GetByIdAsync(command.Id);

            if(task != null)
            {
                task.Reschedule(command.NewDeadline);

                await _repository.UpdateTaskAsync(task);
            }

            return Transformer.ToDto(task);
        }
    }
}