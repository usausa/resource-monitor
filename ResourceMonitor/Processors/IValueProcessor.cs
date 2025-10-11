namespace ResourceMonitor.Processors;

using ResourceMonitor.Models;

public interface IValueProcessor
{
    ValueTask ProcessAsync(MonitorValues values);
}
