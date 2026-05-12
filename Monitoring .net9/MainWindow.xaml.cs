using Monitoring.Services;
using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Monitoring_net9
{
    public partial class MainWindow : Window
    {
        private readonly HardwareMonitorService monitorService;
        private readonly DispatcherTimer timer;
        public ISeries[] GpuTempSeries { get; set; }

        private ObservableCollection<double> gpuTempValues = new();


        public MainWindow()
        {
            InitializeComponent();

            monitorService = new HardwareMonitorService();
            GpuTempSeries =
            [
                new LineSeries<double>
                {
                    Values = gpuTempValues
                }
            ];

            DataContext = this;

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
            TimeText.Text = DateTime.Now.ToString("HH:mm:ss");

            DateText.Text = DateTime.Now.ToString(
                "dddd dd MMMM yyyy",
                new CultureInfo("fr-FR"));

            monitorService.Update();

            CpuUsageText.Text =
                $"{monitorService.CpuUsage:F1} %";

            RamUsageText.Text =
                $"{monitorService.RamUsed:F1} GB";

            GpuUsageText.Text =
                $"{monitorService.GpuUsage:F1} %";

            GpuTempText.Text =
                $"{monitorService.GpuTemp:F1} °C";

            gpuTempValues.Add(monitorService.GpuTemp);

            if (gpuTempValues.Count > 60)
            {
                gpuTempValues.RemoveAt(0);
            }

            GpuMemoryText.Text =
                $"{monitorService.GpuMemoryUsedGB:F1} GB";
        }
    }
}