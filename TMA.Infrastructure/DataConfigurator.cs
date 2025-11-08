using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TMA.Application;

namespace TMA.Infrastructure;

public static class DataConfigurator
{
    public static IServiceCollection ConfigureData(this IServiceCollection services, IConfiguration configuration) => services
        .AddDbContext<AppDbContext>(o => o.UseSqlite(configuration.GetConnectionString("Default")))
        .AddScoped<ITaskRepository, TaskRepository>()
        .AddScoped<IUnitOfWork, UnitOfWork>();
}