namespace ResourceMonitor;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using ResourceMonitor.Hubs;
using ResourceMonitor.Views;

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
        // TODO
        // Services

        // Window
        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<MainWindowViewModel>();

        return builder;
    }
}
