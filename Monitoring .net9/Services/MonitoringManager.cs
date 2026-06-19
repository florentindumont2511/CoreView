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

        public bool IsHwInfoRestartDue(TimeSpan maximumUptime)
        {
            try
            {
                DateTime now = DateTime.Now;
                Process[] processes =
                    Process.GetProcessesByName(HwInfoProcessName);

                try
                {
                    foreach (Process process in processes)
                    {
                        if (now - process.StartTime >= maximumUptime)
                        {
                            return true;
                        }
                    }

                    return false;
                }
                finally
                {
                    foreach (Process process in processes)
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.Log($"HWiNFO uptime check error: {ex.Message}");
                return false;
            }
        }

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
                    await process.WaitForExitAsync();
                    process.Dispose();
                }
                catch (Exception ex)
                {
                    LoggerService.Log($"HWiNFO kill error: {ex.Message}");
                }
            }

            if (Process.GetProcessesByName(HwInfoProcessName).Length > 0)
            {
                await RunElevatedTaskKillAsync();
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
            StartHwInfo();
            await Task.Delay(TimeSpan.FromSeconds(5));
            ConnectHwInfo();
        }

        private static async Task RunElevatedTaskKillAsync()
        {
            try
            {
                using Process? taskKill = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "taskkill.exe",
                        Arguments = $"/F /IM {HwInfoProcessName}.exe",
                        UseShellExecute = true,
                        Verb = "runas",
                        WindowStyle = ProcessWindowStyle.Hidden
                    });

                if (taskKill != null)
                {
                    await taskKill.WaitForExitAsync();
                }
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Elevated HWiNFO stop error: {ex.Message}");
            }
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
            Data.SourceDetails.Clear();
            Data.CpuName = hardwareMonitorService.Data.CpuName;
            Data.GpuName = hardwareMonitorService.Data.GpuName;

            Data.CpuUsage = CopyHardwareValue("CpuUsage", 0, 100, true);
            Data.RamUsed = CopyHardwareValue("RamUsed", 0, 4096, true);
            Data.RamTotal = CopyHardwareValue("RamTotal", 0.1, 4096);
            Data.RamUsagePercent =
                CopyHardwareValue("RamUsagePercent", 0, 100, true);
            Data.GpuUsage = CopyHardwareValue("GpuUsage", 0, 100, true);
            Data.GpuMemoryUsedGB =
                CopyHardwareValue("GpuMemoryUsed", 0, 256, true);
            Data.GpuMemoryTotalGB =
                CopyHardwareValue("GpuMemoryTotal", 0.1, 256);
            Data.GpuMemoryUsagePercent =
                CalculatePercent(
                    Data.GpuMemoryUsedGB,
                    Data.GpuMemoryTotalGB,
                    hardwareMonitorService.Data.GpuMemoryUsagePercent);

            Data.SetSource(
                "GpuMemoryUsagePercent",
                IsPlausible(Data.GpuMemoryUsedGB, 0, 256, true) &&
                IsPlausible(Data.GpuMemoryTotalGB, 0.1, 256)
                    ? "Calculé • VRAM utilisée / VRAM totale"
                    : SourceOf(
                        hardwareMonitorService.Data,
                        "GpuMemoryUsagePercent"));

            Data.CpuTemperature =
                SelectPreferred("CpuTemperature", 1, 115);
            Data.CpuClock = SelectPreferred("CpuClock", 100, 10000);
            Data.CpuPower = SelectPreferred("CpuPower", 0.1, 2000);
            Data.CpuTension = SelectPreferred("CpuTension", 0.05, 3);
            Data.RamClock = SelectPreferred("RamClock", 100, 10000);
            Data.GpuTemperature =
                SelectPreferred("GpuTemperature", 1, 125);
            Data.GpuClock = SelectPreferred("GpuClock", 0, 5000, true);
            Data.GpuHotspot = SelectPreferred("GpuHotspot", 1, 130);
            Data.GpuMemoryJunction =
                SelectPreferred("GpuMemoryJunction", 1, 130);
            Data.GpuPower = SelectPreferred("GpuPower", 0.1, 2000);
            Data.GpuTension = SelectPreferred("GpuTension", 0.01, 3);
            Data.Fps = SelectPreferred("Fps", 0, 2000, true);
            Data.TotalPower = Data.CpuPower + Data.GpuPower;
            Data.SetSource(
                "TotalPower",
                "Calculé • puissance CPU + puissance GPU");
        }

        private double CopyHardwareValue(
            string metricId,
            double minimum,
            double maximum,
            bool allowZero = false)
        {
            double value = GetMetricValue(hardwareMonitorService.Data, metricId);

            if (!hardwareMonitorService.Data.SourceDetails.ContainsKey(metricId) ||
                !IsPlausible(value, minimum, maximum, allowZero))
            {
                Data.SetSource(metricId, "Indisponible • aucune source valide");
                return 0;
            }

            Data.SetSource(
                metricId,
                SourceOf(hardwareMonitorService.Data, metricId));
            return value;
        }

        private double SelectPreferred(
            string metricId,
            double minimum,
            double maximum,
            bool allowZero = false)
        {
            double hwInfoValue = GetMetricValue(hwInfoService.Data, metricId);

            if (hwInfoService.HasFreshData &&
                hwInfoService.Data.SourceDetails.ContainsKey(metricId) &&
                IsPlausible(hwInfoValue, minimum, maximum, allowZero))
            {
                Data.SetSource(metricId, SourceOf(hwInfoService.Data, metricId));
                return hwInfoValue;
            }

            double fallbackValue =
                GetMetricValue(hardwareMonitorService.Data, metricId);

            if (hardwareMonitorService.Data.SourceDetails.ContainsKey(metricId) &&
                IsPlausible(fallbackValue, minimum, maximum, allowZero))
            {
                Data.SetSource(
                    metricId,
                    $"{SourceOf(hardwareMonitorService.Data, metricId)} • secours");
                return fallbackValue;
            }

            Data.SetSource(metricId, "Indisponible • aucune source valide");
            return 0;
        }

        private static double GetMetricValue(SensorData data, string metricId)
        {
            return metricId switch
            {
                "CpuUsage" => data.CpuUsage,
                "CpuTemperature" => data.CpuTemperature,
                "CpuClock" => data.CpuClock,
                "CpuPower" => data.CpuPower,
                "CpuTension" => data.CpuTension,
                "RamUsed" => data.RamUsed,
                "RamTotal" => data.RamTotal,
                "RamUsagePercent" => data.RamUsagePercent,
                "RamClock" => data.RamClock,
                "GpuUsage" => data.GpuUsage,
                "GpuTemperature" => data.GpuTemperature,
                "GpuMemoryUsed" => data.GpuMemoryUsedGB,
                "GpuMemoryTotal" => data.GpuMemoryTotalGB,
                "GpuMemoryUsagePercent" => data.GpuMemoryUsagePercent,
                "GpuClock" => data.GpuClock,
                "GpuHotspot" => data.GpuHotspot,
                "GpuMemoryJunction" => data.GpuMemoryJunction,
                "GpuPower" => data.GpuPower,
                "GpuTension" => data.GpuTension,
                "Fps" => data.Fps,
                "TotalPower" => data.TotalPower,
                _ => 0
            };
        }

        private static bool IsPlausible(
            double value,
            double minimum,
            double maximum,
            bool allowZero = false)
        {
            return double.IsFinite(value) &&
                   value <= maximum &&
                   (allowZero ? value >= minimum : value > minimum);
        }

        private static string SourceOf(SensorData data, string metricId)
        {
            return data.SourceDetails.TryGetValue(metricId, out string? source)
                ? source
                : "Source non détaillée";
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
