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

            if (Process.GetProcessesByName(HwInfoProcessName).Length > 0)
            {
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

            foreach (var process in Process.GetProcessesByName(HwInfoProcessName))
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
            Data.CpuUsage = hardwareMonitorService.Data.CpuUsage;
            Data.RamUsed = hardwareMonitorService.Data.RamUsed;
            Data.GpuUsage = hardwareMonitorService.Data.GpuUsage;
            Data.GpuMemoryUsedGB = hardwareMonitorService.Data.GpuMemoryUsedGB;

            Data.CpuTemperature = hwInfoService.Data.CpuTemperature;
            Data.CpuClock = hwInfoService.Data.CpuClock;
            Data.CpuPower = hwInfoService.Data.CpuPower;
            Data.CpuTension = hwInfoService.Data.CpuTension;
            Data.GpuTemperature = hwInfoService.Data.GpuTemperature;
            Data.GpuClock = hwInfoService.Data.GpuClock;
            Data.GpuHotspot = hwInfoService.Data.GpuHotspot;
            Data.GpuMemoryJunction = hwInfoService.Data.GpuMemoryJunction;
            Data.GpuPower = hwInfoService.Data.GpuPower;
            Data.GpuTension = hwInfoService.Data.GpuTension;
        }
    }
}
