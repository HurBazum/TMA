using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using TMA.UI.Infrastructure.Stores;
using TMA.UI.ViewModels;
using System.IO;
using TMA.Infrastructure;
using Microsoft.EntityFrameworkCore;
using TMA.Application;
using TMA.Application.CommandHandlers;
using TMA.Application.QueryHandlers;
using TMA.Application.Queries;

namespace TMA.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public static IServiceProvider? Provider { get; private set; }
    public static IConfiguration? Configuration { get; private set; }
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var serviceCollection = new ServiceCollection();
        var b = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, true);

        Configuration = b.Build();

        ConfigureServices(serviceCollection, Configuration);

        Provider = serviceCollection.BuildServiceProvider();

        var nav = Provider.GetRequiredService<INavigationStore>();
        
        //
        nav.Next(Provider.GetRequiredService<TaskListViewModel>());

        var main = new MainWindow
        {
            DataContext = Provider.GetRequiredService<MainViewModel>()
        };

        main.Show();
    }
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration) => services
        .AddDbContext<AppDbContext>(o => o.UseSqlite(configuration.GetConnectionString("Default")))
        .AddTransient<ITaskRepository, TaskRepository>()
        .AddSingleton<INavigationStore, NavigationStore>()
        .AddTransient<AddTaskCommandHandler>()
        .AddTransient<CompleteTaskCommandHandler>()
        .AddTransient<RenameTaskCommandHandler>()
        .AddTransient<ReprioritizeTaskCommandHandler>()
        .AddTransient<RescheduleTaskCommandHandler>()
        .AddTransient<GetTasksQueryHandler>()
        .AddTransient<GetByIdQueryHandler>()
        .AddTransient<FilterTaskQuery>()
        .AddTransient<FilterTaskQueryHandler>()
        .AddTransient<ITaskService<TaskDto>, TaskService>()
        .AddSingleton<MainViewModel>()
        .AddTransient<FilterViewModel>()
        .AddTransient<TaskListViewModel>()
        .AddTransient<CreateUpdateViewModel>()
        .AddTransient<TestViewModel>();        
}