namespace ResourceMonitor;

using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ResourceMonitor.Views;

using Smart.Mvvm.Resolver;

#pragma warning disable CA1001
public sealed partial class App
#pragma warning restore CA1001
{
    private readonly IHost host;

    private readonly ILogger<App> log;

    private readonly NotifyIcon notifyIcon = new();

    public App()
    {
        InitializeComponent();

        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        host = CreateHost();

        log = host.Services.GetRequiredService<ILogger<App>>();
        ResolveProvider.Default.Provider = host.Services;

        var environment = host.Services.GetRequiredService<IHostEnvironment>();
        ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);

        log.InfoStartup();
        log.InfoStartupSettingsRuntime(RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription, RuntimeInformation.RuntimeIdentifier);
        log.InfoStartupSettingsGC(GCSettings.IsServerGC, GCSettings.LatencyMode, GCSettings.LargeObjectHeapCompactionMode);
        log.InfoStartupSettingsThreadPool(workerThreads, completionPortThreads);
        log.InfoStartupApplication(environment.ApplicationName, typeof(App).Assembly.GetName().Version);
        log.InfoStartupEnvironment(environment.EnvironmentName, environment.ContentRootPath);

        Current.DispatcherUnhandledException += (_, ea) => HandleException(ea.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, ea) => HandleException((Exception)ea.ExceptionObject);
    }

    private static WebApplication CreateHost()
    {
        var builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs());

        // Log
        builder.ConfigureLogging();
        // API
        builder.ConfigureApi();
        // Components
        builder.ConfigureComponents();

        var app = builder.Build();

        // API
        app.MapApi();

        return app;
    }

    //--------------------------------------------------------------------------------
    // Lifecycle
    //--------------------------------------------------------------------------------

    // ReSharper disable once AsyncVoidEventHandlerMethod
    protected override async void OnStartup(StartupEventArgs e)
    {
        // Menu
        var menu = new ContextMenuStrip();
        var defaultItem = new ToolStripMenuItem("Show", null, OnShowClick);
        defaultItem.Font = new Font(defaultItem.Font, System.Drawing.FontStyle.Bold);
        menu.Items.Add(defaultItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, OnExitClick);
        notifyIcon.Icon = new Icon("App.ico");
        notifyIcon.Text = "Booting";
        notifyIcon.ContextMenuStrip = menu;
        notifyIcon.MouseDoubleClick += OnShowClick;
        notifyIcon.Visible = true;

        // Window
        MainWindow = host.Services.GetRequiredService<MainWindow>();

        //  Start host
        await host.StartAsync().ConfigureAwait(false);

        // Update
        // TODO log & visual effect ?
        notifyIcon.Text = "Monitoring";
        MainWindow.Show();
    }

    // ReSharper disable once AsyncVoidEventHandlerMethod
    protected override async void OnExit(ExitEventArgs e)
    {
        // Stop host
        await host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
        host.Dispose();

        notifyIcon.Dispose();
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void HandleException(Exception ex)
    {
        log.ErrorUnknownException(ex);

        System.Windows.MessageBox.Show(ex.ToString(), "Unknown error.", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    //--------------------------------------------------------------------------------
    // Handler
    //--------------------------------------------------------------------------------

    private void OnShowClick(object? sender, EventArgs e)
    {
        MainWindow?.Show();
    }

    private void OnExitClick(object? sender, EventArgs e)
    {
        Shutdown();
    }
}
