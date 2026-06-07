using LibreHardwareMonitor.Hardware;
using Monitoring_net9.Models;

namespace Monitoring_net9.Services
{
    public class HardwareMonitorService : IDisposable
    {
        private Computer computer;
        private bool isOpen;
        private DateTime lastResetAttempt = DateTime.MinValue;

        public SensorData Data { get; } = new();

        public HardwareMonitorService()
        {
            computer = CreateComputer();
            OpenComputer();
        }

        public void Update()
        {
            if (!isOpen)
            {
                return;
            }

            try
            {
                foreach (var hardware in computer.Hardware.ToArray())
                {
                    UpdateHardwareSafely(hardware);

                    foreach (var subHardware in hardware.SubHardware.ToArray())
                    {
                        UpdateHardwareSafely(subHardware);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Hardware monitor update error: {ex.Message}");
                ResetComputer();
            }
        }

        public void Dispose()
        {
            if (!isOpen)
            {
                return;
            }

            try
            {
                computer.Close();
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Hardware monitor dispose error: {ex.Message}");
            }
        }

        private static Computer CreateComputer()
        {
            return new Computer
            {
                IsCpuEnabled = true,
                IsMemoryEnabled = true,
                IsGpuEnabled = true
            };
        }

        private void OpenComputer()
        {
            try
            {
                computer.Open();
                isOpen = true;
            }
            catch (Exception ex)
            {
                isOpen = false;
                LoggerService.Log($"Hardware monitor open error: {ex.Message}");
            }
        }

        private void ResetComputer()
        {
            if (DateTime.Now - lastResetAttempt < TimeSpan.FromSeconds(5))
            {
                return;
            }

            lastResetAttempt = DateTime.Now;
            isOpen = false;

            try
            {
                computer.Close();
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Hardware monitor close error: {ex.Message}");
            }

            computer = CreateComputer();
            OpenComputer();
        }

        private void UpdateHardwareSafely(IHardware hardware)
        {
            try
            {
                UpdateHardware(hardware);
            }
            catch (Exception ex)
            {
                LoggerService.Log(
                    $"Hardware update error ({hardware.Name}): {ex.Message}");
                ResetComputer();
            }
        }

        private void UpdateHardware(IHardware hardware)
        {
            UpdateHardwareName(hardware);
            hardware.Update();

            foreach (var sensor in hardware.Sensors.ToArray())
            {
                ReadSensor(hardware.HardwareType, sensor);
            }
        }

        private void UpdateHardwareName(IHardware hardware)
        {
            if (hardware.HardwareType == HardwareType.Cpu &&
                string.IsNullOrWhiteSpace(Data.CpuName))
            {
                Data.CpuName = hardware.Name;
            }
            else if ((hardware.HardwareType == HardwareType.GpuNvidia ||
                      hardware.HardwareType == HardwareType.GpuAmd) &&
                     string.IsNullOrWhiteSpace(Data.GpuName))
            {
                Data.GpuName = hardware.Name;
            }
        }

        private void ReadSensor(
            HardwareType hardwareType,
            ISensor sensor)
        {
            if (sensor.Value is float value &&
                (float.IsNaN(value) || float.IsInfinity(value)))
            {
                return;
            }

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

            if (sensor.SensorType == SensorType.Load &&
                sensor.Name.Contains("Memory"))
            {
                Data.RamUsagePercent = sensor.Value ?? 0;
            }

            if ((sensor.SensorType == SensorType.Data ||
                 sensor.SensorType == SensorType.SmallData) &&
                sensor.Name.Contains("Memory Available"))
            {
                double available = sensor.Value ?? 0;

                if (Data.RamUsed > 0 && available > 0)
                {
                    Data.RamTotal = Data.RamUsed + available;
                }
            }

            if (Data.RamTotal <= 0 &&
                Data.RamUsed > 0 &&
                Data.RamUsagePercent > 0)
            {
                Data.RamTotal = Data.RamUsed / (Data.RamUsagePercent / 100);
            }
        }

        private void ReadGpuSensor(ISensor sensor)
        {
            if (sensor.SensorType == SensorType.Load &&
                sensor.Name == "GPU Core")
            {
                Data.GpuUsage = sensor.Value ?? 0;
            }

            if (sensor.SensorType == SensorType.Load &&
                sensor.Name.Contains("GPU Memory"))
            {
                Data.GpuMemoryUsagePercent = sensor.Value ?? 0;
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

            if ((sensor.SensorType == SensorType.SmallData ||
                 sensor.SensorType == SensorType.Data) &&
                sensor.Name.Contains("GPU Memory Total"))
            {
                Data.GpuMemoryTotalGB = (sensor.Value ?? 0) / 1024f;
            }

            if (Data.GpuMemoryTotalGB <= 0 &&
                Data.GpuMemoryUsedGB > 0 &&
                Data.GpuMemoryUsagePercent > 0)
            {
                Data.GpuMemoryTotalGB =
                    Data.GpuMemoryUsedGB / (Data.GpuMemoryUsagePercent / 100);
            }
        }
    }
}
