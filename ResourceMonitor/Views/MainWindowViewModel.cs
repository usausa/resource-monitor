namespace ResourceMonitor.Views;

using ResourceMonitor.Events;
using ResourceMonitor.Models;

using Smart.Mvvm.Messaging;
using Smart.Reactive;
using Smart.Windows.Input;

public sealed class MainWindowViewModel : ExtendViewModelBase
{
    public StatDataSet CpuLoadTotalSet { get; }
    public StatDataSet MemoryLoadPhysicalSet { get; }
    public StatDataSet GpuLoadCoreSet { get; }
    public StatDataSet GpuLoadMemorySet { get; }
    public StatDataSet GpuMemoryLoadSet { get; }
    public StatDataSet CpuTemperaturePackageSet { get; }
    public StatDataSet GpuTemperatureCoreSet { get; }
    public StatDataSet CpuPowerPackageSet { get; }
    public StatDataSet GpuPowerPackageSet { get; }

    public EventRequest HideRequest { get; } = new();

    public IObserveCommand HideCommand { get; }

    public MainWindowViewModel(EventBus eventBus)
    {
        CpuLoadTotalSet = new StatDataSet(101);
        MemoryLoadPhysicalSet = new StatDataSet(101);
        GpuLoadCoreSet = new StatDataSet(101);
        GpuLoadMemorySet = new StatDataSet(101);
        GpuMemoryLoadSet = new StatDataSet(101);
        CpuTemperaturePackageSet = new StatDataSet(101);
        GpuTemperatureCoreSet = new StatDataSet(101);
        CpuPowerPackageSet = new StatDataSet(101);
        GpuPowerPackageSet = new StatDataSet(101);

        HideCommand = MakeDelegateCommand(() => HideRequest.Request());

        Disposables.Add(Observable
            .FromEvent<Action<MonitorValues>, MonitorValues>(h => eventBus.Changed += h, h => eventBus.Changed -= h)
            .ObserveOnCurrentContext()
            .Subscribe(x =>
            {
                CpuLoadTotalSet.Add(x.CpuLoadTotal);
                MemoryLoadPhysicalSet.Add(x.MemoryLoadPhysical);
                GpuLoadCoreSet.Add(x.GpuLoadCore);
                GpuLoadMemorySet.Add(x.GpuLoadMemory);
                GpuMemoryLoadSet.Add(x.GpuMemoryLoad);
                CpuTemperaturePackageSet.Add(x.CpuTemperaturePackage);
                GpuTemperatureCoreSet.Add(x.GpuTemperatureCore);
                CpuPowerPackageSet.Add(x.CpuPowerPackage);
                GpuPowerPackageSet.Add(x.GpuPowerPackage);
            }));
    }
}
