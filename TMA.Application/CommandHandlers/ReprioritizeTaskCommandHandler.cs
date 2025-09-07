using TMA.Application.Commands;

namespace TMA.Application.CommandHandlers
{
    public class ReprioritizeTaskCommandHandler(ITaskRepository repository)
    {
        private readonly ITaskRepository _repository = repository;

        public async Task Handle(ReprioritizeTaskCommand command)
        {
            var task = await _repository.GetByIdAsync(command.Id);

            if(task != null)
            {
                task.Reprioritize(command.Priority);

                await _repository.UpdateTaskAsync(task);
            }
        }
    }
}