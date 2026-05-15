using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Monitoring.Services;
using Monitoring_net9.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;


namespace Monitoring_net9
{
    public partial class MainWindow : Window
    {
        private readonly MonitoringManager monitoringManager;

        private readonly DispatcherTimer timer;
        public ISeries[] GpuTempSeries { get; set; }

        private ObservableCollection<double> gpuTempValues = new();

        private readonly DispatcherTimer hwInfoRestartTimer;

        private void StartHwInfo()
        {
            var existingProcess =
                Process.GetProcessesByName("HWiNFO64");

            if (existingProcess.Length > 0)
            {
                return;
            }

            string hwinfoPath =
                @"C:\Program Files\HWiNFO64\HWiNFO64.EXE";

            if (File.Exists(hwinfoPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = hwinfoPath,
                    UseShellExecute = true,
                    Verb = "runas"
                });
            }
        }

        private void RestartHwInfo()
        {
            var processes =
                Process.GetProcessesByName("HWiNFO64");

            foreach (var process in processes)
            {
                process.Kill();
            }

            Thread.Sleep(3000);

            StartHwInfo();

            Thread.Sleep(5000);

            monitoringManager.Initialize();
        }

        public MainWindow()
        {
            InitializeComponent();



            monitoringManager = new MonitoringManager();

            monitoringManager.Initialize();

            StartHwInfo();

            Thread.Sleep(5000);


            string result = "";


            foreach (var reading in monitoringManager.Readings)
            {
                if (reading.LabelOrig.Contains(
                    "GPU",
                    StringComparison.OrdinalIgnoreCase))
                {
                    result +=
                        $"{reading.LabelOrig} = {reading.Value} {reading.Unit}\n";
                }
            }

            MessageBox.Show(result);

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


            // TIMER RESTART HWiNFO
            hwInfoRestartTimer = new DispatcherTimer();


            hwInfoRestartTimer.Interval = TimeSpan.FromHours(11);

            hwInfoRestartTimer.Tick += HwInfoRestartTimer_Tick;

            hwInfoRestartTimer.Start();

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


            monitoringManager.Update();

            CpuUsageText.Text =
                $"{monitoringManager.Data.CpuUsage:F1} %";


            CpuTempText.Text =
                $"{monitoringManager.Data.CpuTemperature:F1} °C";
            
            if (monitoringManager.Data.CpuTemperature > 90)
            {
                CpuTempText.Foreground = Brushes.Red;
            }
            else if (monitoringManager.Data.CpuTemperature > 70)
            {
                CpuTempText.Foreground = Brushes.Orange;
            }
            else
            {
                CpuTempText.Foreground = Brushes.White;
            }

            CpuClockText.Text =
                $"{monitoringManager.Data.CpuClock / 1000:F2} GHz";

            CpuPowerText.Text =
                $"{monitoringManager.Data.CpuPower:F1} W";

            RamUsageText.Text =
                $"{monitoringManager.Data.RamUsed:F1} GB";

            GpuUsageText.Text =
                $"{monitoringManager.Data.GpuUsage:F1} %";

            GpuTempText.Text =
                $"{monitoringManager.Data.GpuTemperature:F1} °C";

            gpuTempValues.Add(monitoringManager.Data.GpuTemperature);

            if (gpuTempValues.Count > 60)
            {
                gpuTempValues.RemoveAt(0);
            }

            if (monitoringManager.Data.GpuTemperature > 95)
            {
                GpuTempText.Foreground = Brushes.Red;
            }
            else if (monitoringManager.Data.GpuTemperature > 80)
            {
                GpuTempText.Foreground = Brushes.Orange;
            }
            else
            {
                GpuTempText.Foreground = Brushes.White;
            }

            GpuMemoryText.Text =
                $"{monitoringManager.Data.GpuMemoryUsedGB:F1} GB";

            GpuClockText.Text =
                $"{monitoringManager.Data.GpuClock:F0} MHz";

            GpuHotspotText.Text =
                $"{monitoringManager.Data.GpuHotspot:F1} °C";

            GpuMemoryJunctionText.Text =
                $"{monitoringManager.Data.GpuMemoryJunction:F1} °C";

        }

        private void HwInfoRestartTimer_Tick(
            object? sender,
            EventArgs e)
        {
            RestartHwInfo();
        }

    }
}