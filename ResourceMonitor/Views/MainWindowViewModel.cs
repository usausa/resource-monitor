namespace ResourceMonitor.Views;

using Smart.Mvvm.Messaging;
using Smart.Windows.Input;

public sealed class MainWindowViewModel : ExtendViewModelBase
{
    public EventRequest HideRequest { get; } = new();

    public IObserveCommand HideCommand { get; }

    public MainWindowViewModel()
    {
        HideCommand = MakeDelegateCommand(() => HideRequest.Request());
    }
}
