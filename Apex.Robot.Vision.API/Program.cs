using Serilog;
using Serilog.Events;

namespace Apex.Robot.Vision
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("System", LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
            .WriteTo.Console()
            .CreateLogger();

            try
            {
                Log.Information("Starting up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hostContext, config, loggerConfiguration) => loggerConfiguration
                    .ReadFrom.Configuration(hostContext.Configuration))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseUrls("http://0.0.0.0:5555")
                        .UseStartup<Startup>();
                });
    }
}
