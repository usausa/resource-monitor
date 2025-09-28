namespace ResourceMonitor.Workers;

using LibreHardwareMonitor.Hardware;

using Microsoft.Extensions.Hosting;

using ResourceMonitor.Models;
using ResourceMonitor.Processors;

internal sealed class CollectWorker : BackgroundService
{
    private readonly ILogger<CollectWorker> log;

    private readonly IValueProcessor[] processors;

    private readonly Computer computer;

    private readonly UpdateVisitor updateVisitor = new();

    private readonly List<Action<MonitorValues>> collectActions = new();

    // TODO setting : interval
    public CollectWorker(
        ILogger<CollectWorker> log,
        IEnumerable<IValueProcessor> processors)
    {
        this.log = log;
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));
        try
        {
            do
            {
                // TODO
                System.Diagnostics.Debug.WriteLine("*");

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
                            await processor.ProcessAsync(values);
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

    //--------------------------------------------------------------------------------
    // Sensor
    //--------------------------------------------------------------------------------

    private void SetupSensors()
    {
        // TODO log ?
        var cpuLoadTotal = default(ISensor);
        foreach (var sensor in EnumerateSensors(HardwareType.Cpu, SensorType.Load))
        {
            System.Diagnostics.Debug.WriteLine($"cpu.load : {sensor.Hardware?.Name} - {sensor.Name} - {sensor.Value}");
            if (sensor.Name.Equals("CPU Total", StringComparison.OrdinalIgnoreCase))
            {
                cpuLoadTotal = sensor;
            }
        }

        if (cpuLoadTotal is not null)
        {
            collectActions.Add(x => x.CpuLoadTotal = cpuLoadTotal.Value);
        }

        var cpuTemperaturePackage = default(ISensor);
        foreach (var sensor in EnumerateSensors(HardwareType.Cpu, SensorType.Temperature))
        {
            System.Diagnostics.Debug.WriteLine($"cpu.temperature : {sensor.Hardware?.Name} - {sensor.Name} - {sensor.Value}");
            if (sensor.Name.Equals("CPU Package", StringComparison.OrdinalIgnoreCase) || sensor.Name.Equals("Core (Tctl/Tdie)", StringComparison.OrdinalIgnoreCase))
            {
                cpuTemperaturePackage = sensor;
            }
        }

        if (cpuTemperaturePackage is not null)
        {
            collectActions.Add(x => x.CpuTemperaturePackage = cpuTemperaturePackage.Value);
        }

        var cpuPowerPackage = default(ISensor);
        foreach (var sensor in EnumerateSensors(HardwareType.Cpu, SensorType.Power))
        {
            System.Diagnostics.Debug.WriteLine($"cpu.power : {sensor.Hardware?.Name} - {sensor.Name} - {sensor.Value}");
            if (sensor.Name.Equals("CPU Package", StringComparison.OrdinalIgnoreCase) || sensor.Name.Equals("Package", StringComparison.OrdinalIgnoreCase))
            {
                cpuPowerPackage = sensor;
            }
        }

        if (cpuPowerPackage is not null)
        {
            collectActions.Add(x => x.CpuPowerPackage = cpuPowerPackage.Value);
        }

        var gpuLoadCore = default(ISensor);
        var gpuLoadMemory = default(ISensor);
        foreach (var sensor in EnumerateGpuSensors(SensorType.Load))
        {
            System.Diagnostics.Debug.WriteLine($"gpu.load : {sensor.Hardware?.Name} - {sensor.Name} - {sensor.Value}");
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
            collectActions.Add(x => x.GpuLoadCore = gpuLoadCore.Value);
        }
        if (gpuLoadMemory is not null)
        {
            collectActions.Add(x => x.GpuLoadMemory = gpuLoadMemory.Value);
        }

        var gpuTemperatureCore = default(ISensor);
        foreach (var sensor in EnumerateGpuSensors(SensorType.Temperature))
        {
            System.Diagnostics.Debug.WriteLine($"gpu.temperature : {sensor.Hardware?.Name} - {sensor.Name} - {sensor.Value}");
            if (sensor.Name.Equals("GPU Core", StringComparison.OrdinalIgnoreCase))
            {
                gpuTemperatureCore = sensor;
            }
        }

        if (gpuTemperatureCore is not null)
        {
            collectActions.Add(x => x.GpuTemperatureCore = gpuTemperatureCore.Value);
        }

        var gpuPowerPackage = default(ISensor);
        foreach (var sensor in EnumerateGpuSensors(SensorType.Power))
        {
            System.Diagnostics.Debug.WriteLine($"gpu.power : {sensor.Hardware?.Name} - {sensor.Name} - {sensor.Value}");
            if (sensor.Name.Equals("GPU Package", StringComparison.OrdinalIgnoreCase))
            {
                gpuPowerPackage = sensor;
            }
        }

        if (gpuPowerPackage is not null)
        {
            collectActions.Add(x => x.GpuPowerPackage = gpuPowerPackage.Value);
        }

        var gpuMemoryUsed = default(ISensor);
        var gpuMemoryTotal = default(ISensor);
        foreach (var sensor in EnumerateGpuSensors(SensorType.SmallData))
        {
            System.Diagnostics.Debug.WriteLine($"gpu.memory : {sensor.Hardware?.Name} - {sensor.Name} - {sensor.Value}");
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
                x.GpuMemoryLoad = used.HasValue && total > 0 ? (used / total) * 100 : 0;
            });
        }

        var memoryLoadPhysical = default(ISensor);
        foreach (var sensor in EnumerateSensors(HardwareType.Memory, SensorType.Load))
        {
            System.Diagnostics.Debug.WriteLine($"memory : {sensor.Hardware?.Name} - {sensor.Name} - {sensor.Value}");
            if (sensor.Name.Equals("Memory", StringComparison.OrdinalIgnoreCase))
            {
                memoryLoadPhysical = sensor;
            }
        }

        if (memoryLoadPhysical is not null)
        {
            collectActions.Add(x => x.MemoryLoadPhysical = memoryLoadPhysical.Value);
        }
    }

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
