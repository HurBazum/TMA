using TMA.UI.ViewModels.Base;

namespace TMA.UI.Infrastructure.Stores;

public interface INavigationStore
{
    public ViewModelBase CurrentViewModel { get; }
    public event Action? CurrentViewModelChanged;
    public void Next(ViewModelBase viewModel);
    public void Previous();
}