namespace ResourceMonitor;

internal static class Log
{
    // TODO https://github.com/dotnet/wpf/issues/9589
#pragma warning disable CA1727
#pragma warning disable CA1848

    // Startup

    public static void InfoStartup(this ILogger logger) =>
        logger.LogInformation("Application start.");

    public static void InfoStartupSettingsRuntime(this ILogger logger, string osDescription, string frameworkDescription, string runtimeIdentifier) =>
        logger.LogInformation("Runtime: os=[{osDescription}], framework=[{frameworkDescription}], rid=[{runtimeIdentifier}]", osDescription, frameworkDescription, runtimeIdentifier);

    public static void InfoStartupSettingsGC(this ILogger logger, bool isServerGC, GCLatencyMode latencyMode, GCLargeObjectHeapCompactionMode largeObjectHeapCompactionMode) =>
        logger.LogInformation("GCSettings: serverGC=[{isServerGC}], latencyMode=[{latencyMode}], largeObjectHeapCompactionMode=[{largeObjectHeapCompactionMode}]", isServerGC, latencyMode, largeObjectHeapCompactionMode);

    public static void InfoStartupSettingsThreadPool(this ILogger logger, int workerThreads, int completionPortThreads) =>
        logger.LogInformation("ThreadPool: workerThreads=[{workerThreads}], completionPortThreads=[{completionPortThreads}]", workerThreads, completionPortThreads);

    public static void InfoStartupApplication(this ILogger logger, string application, Version? version) =>
        logger.LogInformation("Application: application=[{application}], version=[{version}]", application, version);

    public static void InfoStartupEnvironment(this ILogger logger, string environment, string contentRoot) =>
        logger.LogInformation("Environment: environment=[{environment}], contentRoot=[{contentRoot}]", environment, contentRoot);

    // Error

    public static void ErrorUnknownException(this ILogger logger, Exception ex) =>
        logger.LogError(ex, "Unknown exception.");

#pragma warning restore CA1848
#pragma warning restore CA1727
}
