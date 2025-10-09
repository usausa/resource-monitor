namespace ResourceMonitor;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using ResourceMonitor.Events;
using ResourceMonitor.Hubs;
using ResourceMonitor.Processors;
using ResourceMonitor.Settings;
using ResourceMonitor.Views;
using ResourceMonitor.Workers;

using Serilog;

internal static class ApplicationExtensions
{
    //--------------------------------------------------------------------------------
    // Logging
    //--------------------------------------------------------------------------------

    internal static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
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

    internal static WebApplicationBuilder ConfigureApi(this WebApplicationBuilder builder, ProcessorOption option)
    {
        if (option.EnableHub)
        {
            builder.Services.AddSignalR();
        }

        return builder;
    }

    internal static void MapApi(this WebApplication app, ProcessorOption option)
    {
        if (option.EnableHub)
        {
            app.UseRouting();

            app.MapHub<MonitorHub>("/monitor");
            app.MapGet("/", () => "Resource monitor");
        }
    }

    //--------------------------------------------------------------------------------
    // Components
    //--------------------------------------------------------------------------------

    internal static WebApplicationBuilder ConfigureComponents(this WebApplicationBuilder builder, ProcessorOption option)
    {
        // Setting
        builder.Services.Configure<MonitorSetting>(builder.Configuration.GetSection("Monitor"));
        builder.Services.AddSingleton(p => p.GetRequiredService<IOptions<MonitorSetting>>().Value);
        builder.Services.Configure<WindowSetting>(builder.Configuration.GetSection("Window"));
        builder.Services.AddSingleton(p => p.GetRequiredService<IOptions<WindowSetting>>().Value);

        // EventBus
        builder.Services.AddSingleton(EventBus.Default);

        // Processors
        builder.Services.AddSingleton<IValueProcessor, EventBusValueProcessor>();
        if (option.EnableHub)
        {
            builder.Services.AddSingleton<IValueProcessor, HubValueProcessor>();
        }
        if (option.EnableLog)
        {
            builder.Services.AddSingleton<IValueProcessor, LogValueProcessor>();
        }

        // Workers
        builder.Services.AddHostedService<CollectWorker>();

        // Window
        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<MainWindowViewModel>();

        return builder;
    }
}
