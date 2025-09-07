using TMA.Application.Commands;

namespace TMA.Application.CommandHandlers
{
    public class RescheduleTaskCommandHandler(ITaskRepository repository)
    {
        private readonly ITaskRepository _repository = repository;

        public async Task Handle(RescheduleTaskCommand command)
        {
            var task = await _repository.GetByIdAsync(command.Id);

            if(task != null)
            {
                task.Reschedule(command.NewDeadline);

                await _repository.UpdateTaskAsync(task);
            }
        }
    }
}