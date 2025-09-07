using System.Windows.Input;
using TMA.UI.Infrastructure.Commands.Base;

namespace TMA.UI.Infrastructure.Commands;

public class AsyncCommand(Func<object, Task> execute, Func<object, bool> canExecute = null) : IAsyncCommand
{
    private bool _isExecuting;
    public event EventHandler? CanExecutedChanged;

    public bool CanExecute(object? parameter) => !_isExecuting && (canExecute?.Invoke(parameter) ?? true);
    public async Task ExecuteAsync(object? parameter)
    {
        _isExecuting = true;
        OnCanExecutedChanged();

        try
        {
            await execute(parameter);
        }
        finally
        {
            _isExecuting = false;
            OnCanExecutedChanged();
        }
    }

    protected void OnCanExecutedChanged() => CanExecutedChanged?.Invoke(this, EventArgs.Empty);
}