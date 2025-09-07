using TMA.Application.Commands;

namespace TMA.Application.CommandHandlers
{
    public class CompleteTaskCommandHandler(ITaskRepository repository)
    {
        private readonly ITaskRepository _repository = repository;

        public async Task Handle(CompleteTaskCommand command)
        {
            var task = await _repository.GetByIdAsync(command.Id);

            if(task != null)
            {
                task.Complete();

                await _repository.UpdateTaskAsync(task);
            }
        }
    }
}