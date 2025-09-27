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
        private System.Threading.Timer? _precisionTimer;

        private readonly Random _random = new Random();

        private readonly IDisposable disposable;

        //private readonly MonitorObserver observer = new();

        public MainWindow()
        {
            InitializeComponent();

            //_precisionTimer = new System.Threading.Timer(
            //    state =>
            //    {
            //        // TODO stop or dispose

            //        // Update the UI on the dispatcher thread
            //        Application.Current.Dispatcher.Invoke(() =>
            //        {
            //            // Simulate CPU usage between 15-35%
            //            double cpuValue = 15 + _random.NextDouble() * 75;
            //            CpuControl.Value = (float)cpuValue;

            //            // Simulate memory usage between 30-60%
            //            double memValue = 30 + _random.NextDouble() * 70;
            //            MemoryControl.Value = (float)memValue;
            //        });
            //    },
            //    null,
            //    0,
            //    500);

            //observer.Start("http://127.0.0.1:9980/monitor");
            //observer.ValueChanged += data =>
            //{
            //    Application.Current.Dispatcher.Invoke(() =>
            //    {
            //        CpuControl.Value = data.CpuLoadTotal ?? 0;
            //        MemoryControl.Value = data.MemoryLoadPhysical ?? 0;
            //    });
            //};

            disposable = SignalRClient.Observe<MonitorValues>("http://127.0.0.1:9980/monitor", "Receive", data =>
            {
                // TODO Disposed後のチェック
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CpuControl.Value = data.CpuLoadTotal ?? 0;
                    MemoryControl.Value = data.MemoryLoadPhysical ?? 0;
                });
            });
            }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            disposable.Dispose();
            //observer.Stop();

            _precisionTimer?.Dispose();
        }
    }
}
