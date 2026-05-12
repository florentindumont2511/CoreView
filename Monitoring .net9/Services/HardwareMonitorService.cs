using LibreHardwareMonitor.Hardware;

namespace Monitoring.Services
{
    public class HardwareMonitorService
    {
        private readonly Computer computer;

        public float CpuUsage { get; private set; }
        public float RamUsed { get; private set; }
        public float GpuUsage { get; private set; }
        public float GpuTemp { get; private set; }
        public float GpuMemoryUsed { get; private set; }

        public float GpuMemoryUsedGB =>
            GpuMemoryUsed / 1024f;

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
                        CpuUsage = sensor.Value ?? 0;
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
                        RamUsed = sensor.Value ?? 0;
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
                        GpuUsage = sensor.Value ?? 0;
                    }

                    // Température GPU
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        GpuTemp = sensor.Value ?? 0;
                    }

                    // VRAM
                    if ((sensor.SensorType == SensorType.SmallData ||
                         sensor.SensorType == SensorType.Data) &&
                        sensor.Name.Contains("GPU Memory Used"))
                    {
                        GpuMemoryUsed = sensor.Value ?? 0;
                    }
                }
            }
        }
    }
}