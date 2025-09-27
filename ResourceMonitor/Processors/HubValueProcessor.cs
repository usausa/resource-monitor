namespace ResourceMonitor.Processors;

using Microsoft.AspNetCore.SignalR;

using ResourceMonitor.Hubs;
using ResourceMonitor.Models;

internal sealed class HubValueProcessor : IValueProcessor
{
    private readonly IHubContext<MonitorHub> hubContext;

    public HubValueProcessor(IHubContext<MonitorHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async ValueTask ProcessAsync(MonitorValues values)
    {
        await hubContext.Clients.All.SendAsync("Receive", values).ConfigureAwait(false);
    }
}
