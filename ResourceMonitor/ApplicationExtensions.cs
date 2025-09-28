namespace ResourceMonitor;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using ResourceMonitor.Events;
using ResourceMonitor.Hubs;
using ResourceMonitor.Processors;
using ResourceMonitor.Views;
using ResourceMonitor.Workers;

using Serilog;

public static class ApplicationExtensions
{
    //--------------------------------------------------------------------------------
    // Logging
    //--------------------------------------------------------------------------------

    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog(options =>
        {
            options.ReadFrom.Configuration(builder.Configuration);
        });

        return builder;
    }

    //--------------------------------------------------------------------------------
    // API
    //--------------------------------------------------------------------------------

    public static WebApplicationBuilder ConfigureApi(this WebApplicationBuilder builder)
    {
        builder.Services.AddSignalR();

        return builder;
    }

    public static void MapApi(this WebApplication app)
    {
        app.UseRouting();

        app.MapHub<MonitorHub>("/monitor");
        app.MapGet("/", () => "Resource monitor");
    }

    //--------------------------------------------------------------------------------
    // Components
    //--------------------------------------------------------------------------------

    public static WebApplicationBuilder ConfigureComponents(this WebApplicationBuilder builder)
    {
        // TODO setting & processor & window size?
        // TODO debug processor info

        // EventBus
        builder.Services.AddSingleton(EventBus.Default);

        // Processors
        builder.Services.AddSingleton<IValueProcessor, EventBusValueProcessor>();
        builder.Services.AddSingleton<IValueProcessor, HubValueProcessor>();

        // Workers
        builder.Services.AddHostedService<CollectWorker>();

        // Window
        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<MainWindowViewModel>();

        return builder;
    }
}
