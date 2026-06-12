using Monitoring_net9.Models;
using System.Diagnostics;
using System.IO;

namespace Monitoring_net9.Services
{
    public class MonitoringManager : IDisposable
    {
        private const string HwInfoProcessName = "HWiNFO64";

        private readonly HardwareMonitorService hardwareMonitorService;
        private readonly HwInfoService hwInfoService;

        public SensorData Data { get; } = new();

        public bool IsHwInfoConnected =>
            hwInfoService.IsConnected;

        public MonitoringManager()
        {
            hardwareMonitorService = new HardwareMonitorService();
            hwInfoService = new HwInfoService();
        }

        public void Initialize()
        {
            StartHwInfo();
            ConnectHwInfo();
        }

        public void StartHwInfo()
        {
            string hwInfoPath = SettingsService.Load().HwInfoPath;

            try
            {
                if (Process.GetProcessesByName(HwInfoProcessName).Length > 0)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                LoggerService.Log($"HWiNFO process check error: {ex.Message}");
                return;
            }

            if (!File.Exists(hwInfoPath))
            {
                LoggerService.Log($"HWiNFO executable not found: {hwInfoPath}");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = hwInfoPath,
                    UseShellExecute = true,
                    Verb = "runas"
                });

                LoggerService.Log("HWiNFO started");
            }
            catch (Exception ex)
            {
                LoggerService.Log($"HWiNFO start error: {ex.Message}");
            }
        }

        public async Task RestartHwInfoAsync()
        {
            hwInfoService.Disconnect();

            Process[] processes = [];

            try
            {
                processes = Process.GetProcessesByName(HwInfoProcessName);
            }
            catch (Exception ex)
            {
                LoggerService.Log($"HWiNFO process list error: {ex.Message}");
            }

            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    process.Dispose();
                }
                catch (Exception ex)
                {
                    LoggerService.Log($"HWiNFO kill error: {ex.Message}");
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
            StartHwInfo();
            await Task.Delay(TimeSpan.FromSeconds(5));
            ConnectHwInfo();
        }

        public void Update()
        {
            hardwareMonitorService.Update();

            if (!hwInfoService.IsConnected)
            {
                ConnectHwInfo();
            }

            if (hwInfoService.IsConnected &&
                hwInfoService.ReadHeader() &&
                hwInfoService.ReadReadings())
            {
                hwInfoService.UpdateCpuTemperature();
                hwInfoService.UpdateAdvancedSensors();
            }

            MergeData();
        }

        public void Dispose()
        {
            hwInfoService.Disconnect();
            hardwareMonitorService.Dispose();
        }

        private void ConnectHwInfo()
        {
            if (!hwInfoService.Connect())
            {
                return;
            }

            if (!hwInfoService.ReadHeader())
            {
                hwInfoService.Disconnect();
            }
        }

        private void MergeData()
        {
            Data.CpuName = hardwareMonitorService.Data.CpuName;
            Data.GpuName = hardwareMonitorService.Data.GpuName;
            Data.CpuUsage = hardwareMonitorService.Data.CpuUsage;
            Data.RamUsed = hardwareMonitorService.Data.RamUsed;
            Data.RamTotal = hardwareMonitorService.Data.RamTotal;
            Data.RamUsagePercent = hardwareMonitorService.Data.RamUsagePercent;
            Data.GpuUsage = hardwareMonitorService.Data.GpuUsage;
            Data.GpuMemoryUsedGB = hardwareMonitorService.Data.GpuMemoryUsedGB;
            Data.GpuMemoryTotalGB = hardwareMonitorService.Data.GpuMemoryTotalGB;
            Data.GpuMemoryUsagePercent =
                CalculatePercent(
                    Data.GpuMemoryUsedGB,
                    Data.GpuMemoryTotalGB,
                    hardwareMonitorService.Data.GpuMemoryUsagePercent);

            Data.CpuTemperature = hwInfoService.Data.CpuTemperature;
            Data.CpuClock = hwInfoService.Data.CpuClock;
            Data.CpuPower = hwInfoService.Data.CpuPower;
            Data.CpuTension = hwInfoService.Data.CpuTension;
            Data.RamClock = hwInfoService.Data.RamClock;
            Data.GpuTemperature = hwInfoService.Data.GpuTemperature;
            Data.GpuClock = hwInfoService.Data.GpuClock;
            Data.GpuHotspot = hwInfoService.Data.GpuHotspot;
            Data.GpuMemoryJunction = hwInfoService.Data.GpuMemoryJunction;
            Data.GpuPower = hwInfoService.Data.GpuPower;
            Data.GpuTension = hwInfoService.Data.GpuTension;
            Data.Fps = hwInfoService.Data.Fps;
            Data.TotalPower = Data.CpuPower + Data.GpuPower;
        }

        private static double CalculatePercent(
            double used,
            double total,
            double fallback)
        {
            if (double.IsFinite(used) &&
                double.IsFinite(total) &&
                used > 0 &&
                total > 0)
            {
                return Math.Clamp((used / total) * 100, 0, 100);
            }

            return fallback;
        }
    }
}
