namespace ResourceMonitor.Processors;

using ResourceMonitor.Models;

internal sealed class LogValueProcessor : IValueProcessor
{
    private readonly ILogger<LogValueProcessor> log;

    public LogValueProcessor(ILogger<LogValueProcessor> log)
    {
        this.log = log;
    }

#pragma warning disable CA1848
    public ValueTask ProcessAsync(MonitorValues values)
    {
        log.LogInformation(
            "CPU load: {CpuLoad:F1}%, " +
            "CPU temp: {CpuTemperature:F1}C, " +
            "CPU power: {CpuPower:F1}W, " +
            "GPU core load: {GpuLoadCore:F1}%, " +
            "GPU memory load: {GpuLoadMemory:F1}%, " +
            "GPU temp: {GpuTemperature:F1}C, " +
            "GPU power: {GpuPower:F1}W, " +
            "GPU memory: {GpuMemory:F1}%, " +
            "Memory used: {MemoryLoad:F1}%",
            values.CpuLoadTotal,
            values.CpuTemperaturePackage,
            values.CpuPowerPackage,
            values.GpuLoadCore,
            values.GpuLoadMemory,
            values.GpuTemperatureCore,
            values.GpuPowerPackage,
            values.GpuMemoryLoad,
            values.MemoryLoadPhysical);

        return ValueTask.CompletedTask;
    }
#pragma warning restore CA1848
}
