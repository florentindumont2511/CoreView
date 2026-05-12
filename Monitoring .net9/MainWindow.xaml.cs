using Monitoring.Services;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Monitoring
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

        private void Timer_Tick(object? sender, EventArgs e)
        {
            monitorService.Update();

            CpuUsageText.Text =
                $"CPU Usage : {monitorService.CpuUsage:F1} %";

            RamUsageText.Text =
                $"RAM Used : {monitorService.RamUsed:F1} GB";

            GpuUsageText.Text =
                $"GPU Usage : {monitorService.GpuUsage:F1} %";

            GpuTempText.Text =
                $"GPU Temp : {monitorService.GpuTemp:F1} °C";

            GpuMemoryText.Text =
                $"GPU VRAM : {monitorService.GpuMemoryUsedGB:F1} GB";
        }
    }
}