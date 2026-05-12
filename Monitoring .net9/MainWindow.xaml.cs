using Monitoring.Services;
using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;

namespace Monitoring_net9
{
    public partial class MainWindow : Window
    {
        private readonly HardwareMonitorService monitorService;
        private readonly DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            monitorService = new HardwareMonitorService();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Application.Current.Shutdown();
            }

            base.OnKeyDown(e);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            monitorService.Update();

            CpuUsageText.Text =
                $"{monitorService.CpuUsage:F1} %";

            RamUsageText.Text =
                $"{monitorService.RamUsed:F1} GB";

            GpuUsageText.Text =
                $"{monitorService.GpuUsage:F1} %";

            GpuTempText.Text =
                $"{monitorService.GpuTemp:F1} °C";

            GpuMemoryText.Text =
                $"{monitorService.GpuMemoryUsedGB:F1} GB";
        }
    }
}