namespace ResourceMonitor.Processors;

using ResourceMonitor.Models;

internal interface IValueProcessor
{
    ValueTask ProcessAsync(MonitorValues values);
}
