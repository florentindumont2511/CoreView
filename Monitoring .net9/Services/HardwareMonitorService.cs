using LibreHardwareMonitor.Hardware;
using Monitoring_net9.Models;

namespace Monitoring_net9.Services
{
    public class HardwareMonitorService : IDisposable
    {
        private readonly Computer computer;
        private bool isOpen;

        public SensorData Data { get; } = new();

        public HardwareMonitorService()
        {
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsMemoryEnabled = true,
                IsGpuEnabled = true
            };

            try
            {
                computer.Open();
                isOpen = true;
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Hardware monitor open error: {ex.Message}");
            }
        }

        public void Update()
        {
            if (!isOpen)
            {
                return;
            }

            foreach (var hardware in computer.Hardware)
            {
                UpdateHardware(hardware);

                foreach (var subHardware in hardware.SubHardware)
                {
                    UpdateHardware(subHardware);
                }
            }
        }

        public void Dispose()
        {
            if (!isOpen)
            {
                return;
            }

            computer.Close();
        }

        private void UpdateHardware(IHardware hardware)
        {
            hardware.Update();

            foreach (var sensor in hardware.Sensors)
            {
                ReadSensor(hardware.HardwareType, sensor);
            }
        }

        private void ReadSensor(
            HardwareType hardwareType,
            ISensor sensor)
        {
            switch (hardwareType)
            {
                case HardwareType.Cpu:
                    ReadCpuSensor(sensor);
                    break;

                case HardwareType.Memory:
                    ReadMemorySensor(sensor);
                    break;

                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                    ReadGpuSensor(sensor);
                    break;
            }
        }

        private void ReadCpuSensor(ISensor sensor)
        {
            if (sensor.SensorType == SensorType.Load &&
                sensor.Name == "CPU Total")
            {
                Data.CpuUsage = sensor.Value ?? 0;
            }
        }

        private void ReadMemorySensor(ISensor sensor)
        {
            if ((sensor.SensorType == SensorType.Data ||
                 sensor.SensorType == SensorType.SmallData) &&
                sensor.Name.Contains("Memory Used"))
            {
                Data.RamUsed = sensor.Value ?? 0;
            }
        }

        private void ReadGpuSensor(ISensor sensor)
        {
            if (sensor.SensorType == SensorType.Load &&
                sensor.Name == "GPU Core")
            {
                Data.GpuUsage = sensor.Value ?? 0;
            }

            if (sensor.SensorType == SensorType.Temperature)
            {
                Data.GpuTemperature = sensor.Value ?? 0;
            }

            if ((sensor.SensorType == SensorType.SmallData ||
                 sensor.SensorType == SensorType.Data) &&
                sensor.Name.Contains("GPU Memory Used"))
            {
                Data.GpuMemoryUsedGB = (sensor.Value ?? 0) / 1024f;
            }
        }
    }
}
