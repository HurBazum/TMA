using System.Windows.Input;
using TMA.UI.Infrastructure.Commands.Base;

namespace TMA.UI.Infrastructure.Commands;

public class CmdAdapter(IAsyncCommand command) : ICommand
{
    private readonly IAsyncCommand _asyncCommand = command;


    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _asyncCommand.CanExecute(parameter);

    public async void Execute(object? parameter) => await _asyncCommand.ExecuteAsync(parameter);
}