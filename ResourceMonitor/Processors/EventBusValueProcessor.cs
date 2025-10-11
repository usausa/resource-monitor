namespace ResourceMonitor.Processors;

using ResourceMonitor.Events;
using ResourceMonitor.Models;

public sealed class EventBusValueProcessor : IValueProcessor
{
    private readonly EventBus eventBus;

    public EventBusValueProcessor(EventBus eventBus)
    {
        this.eventBus = eventBus;
    }

    public ValueTask ProcessAsync(MonitorValues values)
    {
        eventBus.NotifyChanged(values);
        return ValueTask.CompletedTask;
    }
}
