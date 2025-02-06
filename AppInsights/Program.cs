using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyConsoleApp;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // See appsettings.json for configuration
                // Add ApplicationInsights.config (see https://stackoverflow.com/questions/54772078/application-insights-from-last-debug-session)
                services.AddApplicationInsightsTelemetryWorkerService();

                services.AddHostedService<Worker>();
            });
}


internal sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly TelemetryClient telemetryClient;

    public Worker(ILogger<Worker> logger, TelemetryClient telemetryClient)
    {
        this.logger          = logger;
        this.telemetryClient = telemetryClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            var message = $"Heartbeat at {DateTimeOffset.Now}";
            logger.LogInformation("Information: " + message); // see AppInsights.LogLevel.Default: this will NOT be logged
            logger.LogWarning("Warning: " + message); // see AppInsights.LogLevel.Default: this WILL be logged

            // Send a trace message to Application Insights.
            //telemetryClient.TrackTrace(message);

            // Wait 5 seconds before sending the next telemetry item.
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
