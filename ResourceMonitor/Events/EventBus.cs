namespace ResourceMonitor.Events;

using ResourceMonitor.Models;

internal sealed class EventBus
{
    public static EventBus Default { get; } = new();

    public event Action<MonitorValues>? Changed;

    public void NotifyChanged(MonitorValues values) => Changed?.Invoke(values);
}
