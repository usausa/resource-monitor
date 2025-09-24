namespace ResourceMonitor.Workers;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

using ResourceMonitor.Hubs;

public sealed class CollectWorker : BackgroundService
{
    private readonly ILogger<CollectWorker> log;

    private readonly IHubContext<MonitorHub> hubContext;

    public CollectWorker(
        ILogger<CollectWorker> log,
        IHubContext<MonitorHub> hubContext)
    {
        this.log = log;
        this.hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO
        var counter = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            System.Diagnostics.Debug.WriteLine("*");

            try
            {
                counter++;
                await hubContext.Clients.All.SendAsync("ReceiveData", new CollectValue { Value = counter }, stoppingToken);
            }
            catch (Exception ex)
            {
                log.ErrorUnknownException(ex);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}
