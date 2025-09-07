using TMA.UI.Infrastructure.Stores;
using TMA.UI.ViewModels.Base;

namespace TMA.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(INavigationStore navigationStore)
        {
            _navigationStore = navigationStore;
            _navigationStore.CurrentViewModelChanged += OnChangedViewModel;
        }

        private readonly INavigationStore _navigationStore;

        private string _title = "TMA";
        public string Title 
        { 
            get => _title;
            set => Set(ref _title, value);
        }

        public ViewModelBase ViewModel => _navigationStore.CurrentViewModel;

        private void OnChangedViewModel() => OnPropertyChanged(nameof(ViewModel));
    }
}