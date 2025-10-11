namespace ResourceMonitor.Views;

using ResourceMonitor.Events;
using ResourceMonitor.Models;

using Smart.Mvvm;
using Smart.Mvvm.Messaging;
using Smart.Reactive;
using Smart.Windows.Input;

public sealed partial class MainWindowViewModel : ExtendViewModelBase
{
    [ObservableProperty]
    public partial MonitorValues Values { get; set; } = new();

    public EventRequest HideRequest { get; } = new();

    public IObserveCommand HideCommand { get; }

    public MainWindowViewModel(EventBus eventBus)
    {
        HideCommand = MakeDelegateCommand(() => HideRequest.Request());

        Disposables.Add(Observable
            .FromEvent<Action<MonitorValues>, MonitorValues>(h => eventBus.Changed += h, h => eventBus.Changed -= h)
            .ObserveOnCurrentContext()
            .Subscribe(x => Values = x));
    }
}
