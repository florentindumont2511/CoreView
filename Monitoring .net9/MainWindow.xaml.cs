using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Monitoring.Services;
using Monitoring_net9.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Monitoring_net9
{
    public partial class MainWindow : Window
    {
        private readonly HardwareMonitorService monitorService;
        private readonly DispatcherTimer timer;
        public ISeries[] GpuTempSeries { get; set; }

        private ObservableCollection<double> gpuTempValues = new();

        private readonly HwInfoService hwInfoService;

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

            hwInfoService.Connect();

            hwInfoService.ReadHeader();
        }

        public MainWindow()
        {
            InitializeComponent();

            monitorService = new HardwareMonitorService();

            hwInfoService = new HwInfoService();

            StartHwInfo();

            Thread.Sleep(5000);

            bool connected = hwInfoService.Connect();

            bool headerRead = hwInfoService.ReadHeader();

            bool readingsRead = hwInfoService.ReadReadings();

            if (connected && headerRead && readingsRead)
            {
                string result = "";

                foreach (var reading in hwInfoService.Readings)
                {
                    /*
                    result +=
                        $"{reading.LabelOrig} = {reading.Value} {reading.Unit}\n";*/
                    if (reading.LabelOrig.Contains("Tctl") ||
                        reading.LabelOrig.Contains("Tdie"))
                        {
                            result +=
                            $"{reading.LabelOrig} = {reading.Value} {reading.Unit}\n";
                        }
                }

                MessageBox.Show(result);
            }
            else
            {
                MessageBox.Show("Erreur lecture HWiNFO");
            }

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

            hwInfoService.ReadReadings();
            hwInfoService.UpdateCpuTemperature();

            monitorService.Update();

            CpuUsageText.Text =
                $"{monitorService.CpuUsage:F1} %";

            CpuTempText.Text =
                $"{hwInfoService.CpuTemperature:F1} °C";

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

        private void HwInfoRestartTimer_Tick(
            object? sender,
            EventArgs e)
        {
            RestartHwInfo();
        }

    }
}