using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TMA.UI.Infrastructure.Stores;
using TMA.UI.ViewModels;
using TMA.Infrastructure;
using TMA.Application;
using Microsoft.Extensions.Hosting;

namespace TMA.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private static IHost? _host;
    public static IHost Host => _host ??= Program.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var host = Host;

        await host.StartAsync();

        var nav = host.Services.GetRequiredService<INavigationStore>();
        
        var defaultVm = host.Services.GetRequiredService<TaskListViewModel>();
        //
        nav.Next(defaultVm);

        var main = new MainWindow
        {
            DataContext = Host.Services.GetRequiredService<MainViewModel>()
        };

        main.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _cancellationTokenSource.Cancel();

        base.OnExit(e);

        var host = Host;

        host.StopAsync();

        host.Dispose();
    }

    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration) => services
        .ConfigureData(configuration)
        .ConfigureApplication()
        .ConfigureUI();
}