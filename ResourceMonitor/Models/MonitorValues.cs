namespace ResourceMonitor.Models;

public sealed class MonitorValues
{
    public float CpuLoadTotal { get; set; }

    public float CpuTemperaturePackage { get; set; }

    public float CpuPowerPackage { get; set; }

    public float GpuLoadCore { get; set; }

    public float GpuLoadMemory { get; set; }

    public float GpuTemperatureCore { get; set; }

    public float GpuPowerPackage { get; set; }

    public float GpuMemoryLoad { get; set; }

    public float MemoryLoadPhysical { get; set; }
}
