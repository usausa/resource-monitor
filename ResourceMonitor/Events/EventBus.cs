namespace ResourceMonitor.Events;

using ResourceMonitor.Models;

public sealed class EventBus
{
    public static EventBus Default { get; } = new();

#pragma warning disable CA1003
    public event Action<MonitorValues>? Changed;
#pragma warning restore CA1003

    public void NotifyChanged(MonitorValues values) => Changed?.Invoke(values);
}
