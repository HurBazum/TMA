using Microsoft.Extensions.Hosting;

namespace TMA.UI
{
    public class Program
    {
        [STAThread]
        public static async Task Main(string[] args)
        {
            App app = new();

            app.InitializeComponent();

            app.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostingContext, services) =>
            {
                var config = hostingContext.Configuration;
                App.ConfigureServices(services, config);
            });
    }
}
