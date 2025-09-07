namespace TMA.UI.Infrastructure.Commands.Base;

public interface IAsyncCommand
{
    Task ExecuteAsync(object? parameter);
    bool CanExecute(object? paramater);
    event EventHandler? CanExecutedChanged;
}