using LibreHardwareMonitor.Hardware;
using Monitoring_net9.Models;

namespace Monitoring.Services
{
    public class HardwareMonitorService
    {
        private readonly Computer computer;

        public SensorData Data { get; private set; } = new SensorData();


        public HardwareMonitorService()
        {
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsMemoryEnabled = true,
                IsGpuEnabled = true
            };

            computer.Open();
        }

        public void Update()
        {
            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();

                ReadSensors(hardware);

                foreach (var subHardware in hardware.SubHardware)
                {
                    subHardware.Update();
                    ReadSensors(subHardware);
                }
            }
        }

        private void ReadSensors(IHardware hardware)
        {
            // CPU
            if (hardware.HardwareType == HardwareType.Cpu)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load &&
                        sensor.Name == "CPU Total")
                    {
                        Data.CpuUsage = sensor.Value ?? 0;
                    }
                }
            }

            // RAM
            if (hardware.HardwareType == HardwareType.Memory)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    if ((sensor.SensorType == SensorType.Data ||
                         sensor.SensorType == SensorType.SmallData) &&
                        sensor.Name.Contains("Memory Used"))
                    {
                        Data.RamUsed = sensor.Value ?? 0;
                    }
                }
            }

            // GPU
            if (hardware.HardwareType == HardwareType.GpuNvidia ||
                hardware.HardwareType == HardwareType.GpuAmd)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    // Usage GPU
                    if (sensor.SensorType == SensorType.Load &&
                        sensor.Name == "GPU Core")
                    {
                        Data.GpuUsage = sensor.Value ?? 0;
                    }

                    // Température GPU
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        Data.GpuTemperature = sensor.Value ?? 0;
                    }

                    // VRAM
                    if ((sensor.SensorType == SensorType.SmallData ||
                         sensor.SensorType == SensorType.Data) &&
                        sensor.Name.Contains("GPU Memory Used"))
                    {
                        Data.GpuMemoryUsedGB = (sensor.Value ?? 0) / 1024f;
                    }
                }
            }
        }
    }
}