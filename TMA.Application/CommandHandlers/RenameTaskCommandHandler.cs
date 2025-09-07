using TMA.Application.Commands;

namespace TMA.Application.CommandHandlers
{
    public class RenameTaskCommandHandler(ITaskRepository repository)
    {
        private readonly ITaskRepository _repository = repository;

        public async Task Handle(RenameTaskCommand command)
        {
            var task = await _repository.GetByIdAsync(command.Id);

            if(task != null)
            {
                task.Rename(command.NewTitle);

                await _repository.UpdateTaskAsync(task);
            }
        }
    }
}