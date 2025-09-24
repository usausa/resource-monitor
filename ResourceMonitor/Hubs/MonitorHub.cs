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
        // TODO
        log.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // TODO
        log.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        return Task.CompletedTask;
    }
}
