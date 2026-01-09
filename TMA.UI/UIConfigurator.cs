using Microsoft.Extensions.DependencyInjection;
using TMA.UI.Infrastructure.Stores;
using TMA.UI.ViewModels;

namespace TMA.UI;

public static class UIConfigurator
{
    public static IServiceCollection ConfigureUI(this IServiceCollection services) => services
        .AddSingleton<INavigationStore, NavigationStore>()
        .AddSingleton<MainViewModel>()
        .AddSingleton<FilterViewModel>()
        .AddSingleton<TaskListViewModel>()
        .AddTransient<CreateUpdateViewModel>()
        .AddTransient<TestViewModel>();
}