using Monitoring.Services;
using Monitoring_net9.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            Data.GpuTemperature =
                hardwareMonitorService.Data.GpuTemperature;

            Data.GpuMemoryUsedGB =
                hardwareMonitorService.Data.GpuMemoryUsedGB;

            // Température CPU HWiNFO
            Data.CpuTemperature =
                hwInfoService.Data.CpuTemperature;

            Data.CpuClock =
                hwInfoService.Data.CpuClock;

            Data.CpuPower =
                hwInfoService.Data.CpuPower;

            Data.GpuClock =
                hwInfoService.Data.GpuClock;

            Data.GpuHotspot =
                hwInfoService.Data.GpuHotspot;

            Data.GpuMemoryJunction =
                hwInfoService.Data.GpuMemoryJunction;
        }


    }
}