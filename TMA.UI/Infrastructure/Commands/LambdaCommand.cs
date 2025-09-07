using TMA.UI.Infrastructure.Commands.Base;

namespace TMA.UI.Infrastructure.Commands;

public class LambdaCommand(Action<object> execute, Func<object, bool> canExecute) : Command
{
    private readonly Action<object> _execute = execute;
    private readonly Func<object, bool> _canExecute = canExecute;

    public override bool CanExecute(object? parameter) => _canExecute(parameter);
    public override void Execute(object? parameter) => _execute?.Invoke(parameter);
}