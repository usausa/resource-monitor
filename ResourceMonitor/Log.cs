namespace ResourceMonitor;

internal static class Log
{
    // TODO https://github.com/dotnet/wpf/issues/9589
#pragma warning disable CA1727
#pragma warning disable CA1848

    // Startup

    public static void InfoStartup(this ILogger logger) =>
        logger.LogInformation(message: "Application start.");

    public static void InfoStartupSettingsRuntime(this ILogger logger, string osDescription, string frameworkDescription, string runtimeIdentifier) =>
        logger.LogInformation(message: "Runtime: os=[{osDescription}], framework=[{frameworkDescription}], rid=[{runtimeIdentifier}]", args: [osDescription, frameworkDescription, runtimeIdentifier]);

    public static void InfoStartupSettingsGC(this ILogger logger, bool isServerGC, GCLatencyMode latencyMode, GCLargeObjectHeapCompactionMode largeObjectHeapCompactionMode) =>
        logger.LogInformation(message: "GCSettings: serverGC=[{isServerGC}], latencyMode=[{latencyMode}], largeObjectHeapCompactionMode=[{largeObjectHeapCompactionMode}]", args: [isServerGC, latencyMode, largeObjectHeapCompactionMode]);

    public static void InfoStartupSettingsThreadPool(this ILogger logger, int workerThreads, int completionPortThreads) =>
        logger.LogInformation(message: "ThreadPool: workerThreads=[{workerThreads}], completionPortThreads=[{completionPortThreads}]", args: [workerThreads, completionPortThreads]);

    public static void InfoStartupApplication(this ILogger logger, string application, Version? version) =>
        logger.LogInformation(message: "Application: application=[{application}], version=[{version}]", args: [application, version]);

    public static void InfoStartupEnvironment(this ILogger logger, string environment, string contentRoot) =>
        logger.LogInformation(message: "Environment: environment=[{environment}], contentRoot=[{contentRoot}]", args: [environment, contentRoot]);

    // Error

    public static void ErrorUnknownException(this ILogger logger, Exception ex) =>
        logger.LogError(exception: ex, message: "Unknown exception.");

    // Hub

    public static void InfoConnectionConnected(this ILogger logger, string connectionId) =>
        logger.LogInformation(message: "Client connected: connectionId=[{connectionId}]", args: connectionId);

    public static void InfoConnectionDisconnected(this ILogger logger, string connectionId) =>
        logger.LogInformation(message: "Client disconnected: connectionId=[{connectionId}]", args: connectionId);

#pragma warning restore CA1848
#pragma warning restore CA1727
}
