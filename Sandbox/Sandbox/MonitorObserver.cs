namespace Sandbox;

using Microsoft.AspNetCore.SignalR.Client;

public sealed class CollectValue
{
    public int Value { get; set; }
}

public sealed class MonitorObserver
{
    private readonly HubConnection hubConnection;

    // TODO Event?

    public void Start()
    {
        // TODO
    }

    public void Stop()
    {
        // TODO
    }
}

