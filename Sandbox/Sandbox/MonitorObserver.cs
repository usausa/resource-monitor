using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Threading;

namespace Sandbox;

using Microsoft.AspNetCore.SignalR.Client;

internal sealed class MonitorValues
{
    public float? CpuLoadTotal { get; set; }

    public float? CpuTemperaturePackage { get; set; }

    public float? CpuPowerPackage { get; set; }

    public float? GpuLoadCore { get; set; }

    public float? GpuLoadMemory { get; set; }

    public float? GpuTemperatureCore { get; set; }

    public float? GpuPowerPackage { get; set; }

    public float? GpuMemoryLoad { get; set; }

    public float? MemoryLoadPhysical { get; set; }
}

internal static class SignalRClient
{
    public static IDisposable Observe(string uri, Action<MonitorValues> action)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(uri)
            .WithAutomaticReconnect(new FixedIntervalRetryPolicy(TimeSpan.FromSeconds(5)))
            .Build();

        // Receiveイベントの購読を設定
        connection.On<MonitorValues>("Receive", action);

        // 購読のためのCompositeDisposable
        var disposable = new CompositeDisposable();
        // 接続処理とリトライロジックの実装
        disposable.Add(Observable.FromAsync(async () =>
            {
                await TryConnectWithRetryAsync(connection);
                return true;
            })
            .Subscribe());
        // 接続のクリーンアップのための処理
        disposable.Add(Disposable.Create(async () =>
        {
            if (connection.State == HubConnectionState.Connected)
            {
                await connection.StopAsync();
            }
            await connection.DisposeAsync();
        }));

        return disposable;
    }

    private static async Task TryConnectWithRetryAsync(HubConnection connection)
    {
        bool connected = false;

        while (!connected)
        {
            try
            {
                await connection.StartAsync();
                connected = true;
            }
            catch
            {
                // 接続失敗時、5秒後に再試行
                await Task.Delay(5000);
            }
        }
    }
}

//// System.Reactive.Disposablesの拡張メソッド
//public static class DisposableExtensions
//{
//    public static T AddTo<T>(this T disposable, CompositeDisposable compositeDisposable) where T : IDisposable
//    {
//        compositeDisposable.Add(disposable);
//        return disposable;
//    }
//}

internal sealed class MonitorObserver
{
    private HubConnection? hubConnection;

    public event Action<MonitorValues>? ValueChanged;
    // TODO Event?

    public async void Start(string url)
    {
        if (hubConnection is not null)
        {
            return;
        }

        hubConnection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect(new FixedIntervalRetryPolicy(TimeSpan.FromSeconds(5)))
            .Build();
        // 接続イベントの処理（オプション）
        hubConnection.Reconnecting += error =>
        {
            Debug.WriteLine($"接続が切れました。5秒後に再接続を試行します。エラー: {error?.Message}");
            return Task.CompletedTask;
        };

        hubConnection.Reconnected += connectionId =>
        {
            Debug.WriteLine($"接続が再確立されました。新しいConnection ID: {connectionId}");
            return Task.CompletedTask;
        };

        hubConnection.Closed += error =>
        {
            // IRetryPolicyがnullを返した場合（このポリシーでは発生しない）や、
            // HubConnectionが手動でStopAsync()された場合に発生
            Debug.WriteLine($"接続が閉じられました。エラー: {error?.Message}");
            return Task.CompletedTask;
        };
        hubConnection.On<MonitorValues>("Receive", data => ValueChanged?.Invoke(data));

        // TODO
        try
        {
            await hubConnection.StartAsync();
        }
        catch (Exception e)
        {
            throw;
        }
    }

    public void Stop()
    {
        if (hubConnection is null)
        {
            return;
        }

        hubConnection.StopAsync();
        hubConnection = null;
    }
}

public class FixedIntervalRetryPolicy(TimeSpan retryInterval) : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext) => retryInterval;
}
