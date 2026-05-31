using Monitoring_net9.Models;
using System.Diagnostics;
using System.IO;

namespace Monitoring_net9.Services
{
    public class MonitoringManager : IDisposable
    {
        private const string HwInfoProcessName = "HWiNFO64";
        private const string HwInfoPath = @"C:\Program Files\HWiNFO64\HWiNFO64.EXE";

        private readonly HardwareMonitorService hardwareMonitorService;
        private readonly HwInfoService hwInfoService;

        public SensorData Data { get; } = new();

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
            if (Process.GetProcessesByName(HwInfoProcessName).Length > 0)
            {
                return;
            }

            if (!File.Exists(HwInfoPath))
            {
                LoggerService.Log("HWiNFO executable not found");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = HwInfoPath,
                UseShellExecute = true,
                Verb = "runas"
            });

            LoggerService.Log("HWiNFO started");
        }

        public void RestartHwInfo()
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

            Thread.Sleep(2000);
            StartHwInfo();
            Thread.Sleep(5000);
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
