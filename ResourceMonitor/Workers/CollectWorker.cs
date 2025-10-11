namespace ResourceMonitor.Workers;

using LibreHardwareMonitor.Hardware;

using Microsoft.Extensions.Hosting;

using ResourceMonitor.Models;
using ResourceMonitor.Processors;
using ResourceMonitor.Settings;

public sealed class CollectWorker : BackgroundService
{
    private readonly ILogger<CollectWorker> log;

    private readonly MonitorSetting setting;

    private readonly IValueProcessor[] processors;

    private readonly Computer computer;

    private readonly UpdateVisitor updateVisitor = new();

    private readonly List<Action<MonitorValues>> collectActions = new();

    public CollectWorker(
        ILogger<CollectWorker> log,
        MonitorSetting setting,
        IEnumerable<IValueProcessor> processors)
    {
        this.log = log;
        this.setting = setting;
        this.processors = processors.ToArray();

        computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true
        };
        computer.Open();
        computer.Accept(updateVisitor);

        SetupSensors();
    }

    public override void Dispose()
    {
        base.Dispose();
        computer.Close();
    }

#pragma warning disable CA1031
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(setting.Interval));
        try
        {
            do
            {
                try
                {
                    var values = new MonitorValues();

                    computer.Accept(updateVisitor);

                    foreach (var action in collectActions)
                    {
                        action(values);
                    }

                    foreach (var processor in processors)
                    {
                        try
                        {
                            await processor.ProcessAsync(values).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            log.ErrorUnknownException(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorUnknownException(ex);
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(true));
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }
#pragma warning restore CA1031

    //--------------------------------------------------------------------------------
    // Sensor
    //--------------------------------------------------------------------------------

    private void SetupSensors()
    {
        var cpuLoadTotal = default(ISensor);
        foreach (var sensor in EnumerateSensors(HardwareType.Cpu, SensorType.Load))
        {
            LogFindSensor("cpu.load", sensor);
            if (sensor.Name.Equals("CPU Total", StringComparison.OrdinalIgnoreCase))
            {
                cpuLoadTotal = sensor;
            }
        }

        if (cpuLoadTotal is not null)
        {
            collectActions.Add(x => x.CpuLoadTotal = cpuLoadTotal.Value ?? 0);
        }

        var cpuTemperaturePackage = default(ISensor);
        foreach (var sensor in EnumerateSensors(HardwareType.Cpu, SensorType.Temperature))
        {
            LogFindSensor("cpu.temperature", sensor);
            if (sensor.Name.Equals("CPU Package", StringComparison.OrdinalIgnoreCase) || sensor.Name.Equals("Core (Tctl/Tdie)", StringComparison.OrdinalIgnoreCase))
            {
                cpuTemperaturePackage = sensor;
            }
        }

        if (cpuTemperaturePackage is not null)
        {
            collectActions.Add(x => x.CpuTemperaturePackage = cpuTemperaturePackage.Value ?? 0);
        }

        var cpuPowerPackage = default(ISensor);
        foreach (var sensor in EnumerateSensors(HardwareType.Cpu, SensorType.Power))
        {
            LogFindSensor("cpu.power", sensor);
            if (sensor.Name.Equals("CPU Package", StringComparison.OrdinalIgnoreCase) || sensor.Name.Equals("Package", StringComparison.OrdinalIgnoreCase))
            {
                cpuPowerPackage = sensor;
            }
        }

        if (cpuPowerPackage is not null)
        {
            collectActions.Add(x => x.CpuPowerPackage = cpuPowerPackage.Value ?? 0);
        }

        var gpuLoadCore = default(ISensor);
        var gpuLoadMemory = default(ISensor);
        foreach (var sensor in EnumerateGpuSensors(SensorType.Load))
        {
            LogFindSensor("gpu.load", sensor);
            if (sensor.Name.Equals("GPU Core", StringComparison.OrdinalIgnoreCase))
            {
                gpuLoadCore = sensor;
            }
            if (sensor.Name.Equals("GPU Memory Controller", StringComparison.OrdinalIgnoreCase))
            {
                gpuLoadMemory = sensor;
            }
        }

        if (gpuLoadCore is not null)
        {
            collectActions.Add(x => x.GpuLoadCore = gpuLoadCore.Value ?? 0);
        }
        if (gpuLoadMemory is not null)
        {
            collectActions.Add(x => x.GpuLoadMemory = gpuLoadMemory.Value ?? 0);
        }

        var gpuTemperatureCore = default(ISensor);
        foreach (var sensor in EnumerateGpuSensors(SensorType.Temperature))
        {
            LogFindSensor("gpu.temperature", sensor);
            if (sensor.Name.Equals("GPU Core", StringComparison.OrdinalIgnoreCase))
            {
                gpuTemperatureCore = sensor;
            }
        }

        if (gpuTemperatureCore is not null)
        {
            collectActions.Add(x => x.GpuTemperatureCore = gpuTemperatureCore.Value ?? 0);
        }

        var gpuPowerPackage = default(ISensor);
        foreach (var sensor in EnumerateGpuSensors(SensorType.Power))
        {
            LogFindSensor("gpu.power", sensor);
            if (sensor.Name.Equals("GPU Package", StringComparison.OrdinalIgnoreCase))
            {
                gpuPowerPackage = sensor;
            }
        }

        if (gpuPowerPackage is not null)
        {
            collectActions.Add(x => x.GpuPowerPackage = gpuPowerPackage.Value ?? 0);
        }

        var gpuMemoryUsed = default(ISensor);
        var gpuMemoryTotal = default(ISensor);
        foreach (var sensor in EnumerateGpuSensors(SensorType.SmallData))
        {
            LogFindSensor("gpu.memory", sensor);
            if (sensor.Name.Equals("GPU Memory Used", StringComparison.OrdinalIgnoreCase))
            {
                gpuMemoryUsed = sensor;
            }
            if (sensor.Name.Equals("GPU Memory Total", StringComparison.OrdinalIgnoreCase))
            {
                gpuMemoryTotal = sensor;
            }
        }

        if ((gpuMemoryUsed is not null) && (gpuMemoryTotal is not null))
        {
            collectActions.Add(x =>
            {
                var used = gpuMemoryUsed.Value;
                var total = gpuMemoryTotal.Value;
                x.GpuMemoryLoad = (float)(used.HasValue && total > 0 ? (used / total) * 100 : 0);
            });
        }

        var memoryLoadPhysical = default(ISensor);
        foreach (var sensor in EnumerateSensors(HardwareType.Memory, SensorType.Load))
        {
            LogFindSensor("memory", sensor);
            if (sensor.Name.Equals("Memory", StringComparison.OrdinalIgnoreCase))
            {
                memoryLoadPhysical = sensor;
            }
        }

        if (memoryLoadPhysical is not null)
        {
            collectActions.Add(x => x.MemoryLoadPhysical = memoryLoadPhysical.Value ?? 0);
        }
    }

#pragma warning disable CA1848
    private void LogFindSensor(string category, ISensor sensor)
    {
        log.LogDebug("Find sensor. category=[{Category}], name=[{HardwareName} - {SensorName}], value=[{Value}]", category, sensor.Hardware?.Name, sensor.Name, sensor.Value);
    }
#pragma warning restore CA1848

    //--------------------------------------------------------------------------------
    // Helper
    //--------------------------------------------------------------------------------

    private IEnumerable<ISensor> EnumerateSensors(HardwareType hardwareType, SensorType sensorType) =>
        computer.Hardware
            .SelectMany(EnumerateSensors)
            .Where(x => (x.Hardware.HardwareType == hardwareType) && (x.SensorType == sensorType));

    private IEnumerable<ISensor> EnumerateGpuSensors(SensorType sensorType) =>
        computer.Hardware
            .SelectMany(EnumerateSensors)
            .Where(x => (x.Hardware.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel) && (x.SensorType == sensorType));

    private static IEnumerable<ISensor> EnumerateSensors(IHardware hardware)
    {
        foreach (var subHardware in hardware.SubHardware)
        {
            foreach (var sensor in EnumerateSensors(subHardware))
            {
                yield return sensor;
            }
        }

        foreach (var sensor in hardware.Sensors)
        {
            yield return sensor;
        }
    }
}
