using Monitoring.Services;
using Monitoring_net9.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Monitoring_net9.Services
{
    public class MonitoringManager
    {
        private readonly HardwareMonitorService hardwareMonitorService;

        private readonly HwInfoService hwInfoService;

        public List<HwInfoReadingElement> Readings =>hwInfoService.Readings;

        public SensorData Data { get; private set; } =
            new SensorData();

        public MonitoringManager()
        {
            hardwareMonitorService =
                new HardwareMonitorService();

            hwInfoService =
                new HwInfoService();
        }

        public void StartHwInfo()
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

                LoggerService.Log(
                    "HWiNFO started");
            }
            else
            {
                LoggerService.Log(
                    "HWiNFO executable not found");
            }
        }

        public void RestartHwInfo()
        {
            hwInfoService.Disconnect();

            foreach (var process in
                     Process.GetProcessesByName("HWiNFO64"))
            {
                process.Kill();
            }

            Thread.Sleep(2000);

            Process.Start(
                @"C:\Program Files\HWiNFO64\HWiNFO64.EXE");

            Thread.Sleep(5000);

            hwInfoService.TryReconnect();
        }


        //Méthode qui initialise les providers (HwInfo 64 et autres)
        public void Initialize()
        {
            hwInfoService.Connect();

            hwInfoService.ReadHeader();

            hwInfoService.ReadReadings();
        }

        public void Update()
        {
            // HardwareMonitor
            hardwareMonitorService.Update();

            if (!hwInfoService.IsConnected)
            {
                LoggerService.Log(
                    "HWiNFO disconnected");

                hwInfoService.TryReconnect();

                return;
            }

            // HWiNFO
            hwInfoService.ReadReadings();

            hwInfoService.UpdateCpuTemperature();

            hwInfoService.UpdateAdvancedSensors();

            // Fusion des données
            Data.CpuUsage =
                hardwareMonitorService.Data.CpuUsage;

            Data.RamUsed =
                hardwareMonitorService.Data.RamUsed;

            Data.GpuUsage =
                hardwareMonitorService.Data.GpuUsage;

            Data.GpuMemoryUsedGB =
                hardwareMonitorService.Data.GpuMemoryUsedGB;

            // Température CPU HWiNFO
            Data.CpuTemperature =
                hwInfoService.Data.CpuTemperature;

            Data.CpuClock =
                hwInfoService.Data.CpuClock;

            Data.CpuPower =
                hwInfoService.Data.CpuPower;

            Data.GpuTemperature =
                hwInfoService.Data.GpuTemperature;

            Data.GpuClock =
                hwInfoService.Data.GpuClock;

            Data.GpuHotspot =
                hwInfoService.Data.GpuHotspot;

            Data.GpuMemoryJunction =
                hwInfoService.Data.GpuMemoryJunction;

            Data.GpuPower =
                hwInfoService.Data.GpuPower;
        }
    }
}