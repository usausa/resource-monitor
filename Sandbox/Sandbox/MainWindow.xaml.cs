using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Sandbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IDisposable disposable;

        //private readonly MonitorObserver observer = new();

        public MainWindow()
        {
            InitializeComponent();

            var cpuValues = new StatDataSet(101);
            var memoryValues = new StatDataSet(101);

            CpuControl.DataSet = cpuValues;
            MemoryControl.DataSet = memoryValues;

            disposable = ReactiveSignalR.CreateObservable<MonitorValues>("http://127.0.0.1:9980/monitor", "Receive", TimeSpan.FromSeconds(5))
                    .ObserveOn(SynchronizationContext.Current!)
                    .Subscribe(data =>
                    {
                        cpuValues.Add(data.CpuLoadTotal ?? 0);
                        memoryValues.Add(data.MemoryLoadPhysical ?? 0);
                    });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            disposable.Dispose();
            //observer.Stop();
        }
    }
}
