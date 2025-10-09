namespace ResourceMonitor.Hubs;

using Microsoft.AspNetCore.SignalR;

public sealed class MonitorHub : Hub
{
    private readonly ILogger<MonitorHub> log;

    public MonitorHub(ILogger<MonitorHub> log)
    {
        this.log = log;
    }

    public override Task OnConnectedAsync()
    {
        log.InfoConnectionConnected(Context.ConnectionId);
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        log.InfoConnectionDisconnected(Context.ConnectionId);
        return Task.CompletedTask;
    }
}
