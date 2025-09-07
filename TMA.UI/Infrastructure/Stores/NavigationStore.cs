using Microsoft.Extensions.DependencyInjection;
using TMA.UI.ViewModels;
using TMA.UI.ViewModels.Base;

namespace TMA.UI.Infrastructure.Stores;

public class NavigationStore : INavigationStore
{
    public List<ViewModelBase> ViewModels { get; private set; } = []; 
    public ViewModelBase CurrentViewModel => ViewModels.Last();

    public void Next(ViewModelBase viewModel)
    {
        ViewModels.Add(viewModel);
        OnCurrentViewModelChanged();
    }
    public void Previous()
    {
        if(ViewModels.Count > 1)
        {
            ViewModels.Remove(CurrentViewModel);
            OnCurrentViewModelChanged();
        }
        else
        {
            return;
        }
    }

    public event Action? CurrentViewModelChanged;
    
    private void OnCurrentViewModelChanged() => CurrentViewModelChanged?.Invoke();
}