using System.Windows.Input;
using TMA.UI.Infrastructure.Commands;
using TMA.UI.Infrastructure.Stores;
using TMA.UI.ViewModels.Base;

namespace TMA.UI.ViewModels;



public class TestViewModel(INavigationStore navigationStore) : ViewModelBase
{
    private readonly INavigationStore _navigationStore = navigationStore;
    private string _message = "it is test view model!";
    public string Message
    {
        get => _message;
        set => Set(ref _message, value);
    }

    public ICommand ToTaskCmd => new LambdaCommand(ToTaskCmdExecuted, CanToTaskCmdExecute);

    private bool CanToTaskCmdExecute(object? parameter) => true;
    private void ToTaskCmdExecuted(object? parameter) => _navigationStore.Previous();
}