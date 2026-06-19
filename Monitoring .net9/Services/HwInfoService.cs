using Monitoring_net9.Models;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Monitoring_net9.Services
{
    public class HwInfoService
    {
        private const string SharedMemoryName = "Global\\HWiNFO_SENS_SM2";

        private MemoryMappedFile? memoryFile;
        private DateTime lastConnectErrorLog = DateTime.MinValue;
        private DateTime lastSuccessfulReadUtc = DateTime.MinValue;

        public HwInfoSharedMemHeader Header { get; private set; }

        public SensorData Data { get; } = new();

        public List<HwInfoReadingElement> Readings { get; } = [];

        public Dictionary<uint, HwInfoSensorElement> Sensors { get; } = [];

        public bool IsConnected { get; private set; }

        public bool HasFreshData =>
            IsConnected &&
            DateTime.UtcNow - lastSuccessfulReadUtc < TimeSpan.FromSeconds(5);

        public bool Connect()
        {
            if (memoryFile != null)
            {
                IsConnected = true;
                return true;
            }

            try
            {
                memoryFile =
                    MemoryMappedFile.OpenExisting(SharedMemoryName);

                IsConnected = true;
                LoggerService.Log("HWiNFO connected");

                return true;
            }
            catch (Exception ex)
            {
                LogConnectError(ex);
                IsConnected = false;

                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                memoryFile?.Dispose();
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Disconnect Error: {ex.Message}");
            }
            finally
            {
                memoryFile = null;
                IsConnected = false;
                lastSuccessfulReadUtc = DateTime.MinValue;
            }
        }

        public bool ReadHeader()
        {
            if (memoryFile == null)
            {
                IsConnected = false;
                return false;
            }

            try
            {
                using var accessor = memoryFile.CreateViewAccessor();

                Header = ReadStruct<HwInfoSharedMemHeader>(
                    accessor,
                    0,
                    Marshal.SizeOf<HwInfoSharedMemHeader>());

                return true;
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Error Reading Shared Memory: {ex.Message}");
                Disconnect();

                return false;
            }
        }

        public bool ReadReadings()
        {
            if (memoryFile == null)
            {
                IsConnected = false;
                return false;
            }

            try
            {
                Readings.Clear();
                Sensors.Clear();

                using var accessor = memoryFile.CreateViewAccessor();
                ReadSensors(accessor);
                int elementSize = Marshal.SizeOf<HwInfoReadingElement>();

                for (int i = 0; i < Header.ReadingElementCount; i++)
                {
                    long offset =
                        Header.ReadingSectionOffset +
                        (i * Header.ReadingElementSize);

                    Readings.Add(
                        ReadStruct<HwInfoReadingElement>(
                            accessor,
                            offset,
                            elementSize));
                }

                lastSuccessfulReadUtc = DateTime.UtcNow;

                return true;
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Error Reading Sensor: {ex.Message}");
                Disconnect();

                return false;
            }
        }

        public void UpdateCpuTemperature()
        {
            Data.CpuTemperature = 0;
            Data.SourceDetails.Remove("CpuTemperature");

            var cpuTempReading = Readings.FirstOrDefault(
                r => IsCpuReading(r) && ContainsLabel(r, "Tctl/Tdie"));

            if (!string.IsNullOrEmpty(cpuTempReading.LabelOrig))
            {
                Data.CpuTemperature = cpuTempReading.Value;
                SetSource("CpuTemperature", cpuTempReading);
            }
        }

        public void UpdateAdvancedSensors()
        {
            Data.CpuClock = 0;
            Data.CpuPower = 0;
            Data.CpuTension = 0;
            Data.RamClock = 0;
            Data.GpuTemperature = 0;
            Data.GpuClock = 0;
            Data.GpuHotspot = 0;
            Data.GpuMemoryJunction = 0;
            Data.GpuPower = 0;
            Data.GpuTension = 0;
            Data.Fps = 0;
            foreach (string metricId in new[]
                     {
                         "CpuClock", "CpuPower", "CpuTension", "RamClock",
                         "GpuTemperature", "GpuClock", "GpuHotspot",
                         "GpuMemoryJunction", "GpuPower", "GpuTension", "Fps"
                     })
            {
                Data.SourceDetails.Remove(metricId);
            }

            double gpuPowerCore = 0;
            double gpuPowerSoc = 0;

            foreach (var reading in Readings.Where(r => r.Value > 0))
            {
                UpdateCpuSensor(reading);
                UpdateMemorySensor(reading);
                UpdateGpuSensor(reading, ref gpuPowerCore, ref gpuPowerSoc);
                UpdateFrameRateSensor(reading);
            }

            Data.GpuPower = gpuPowerCore + gpuPowerSoc;

            if (Data.GpuPower > 0)
            {
                Data.SetSource(
                    "GpuPower",
                    "HWiNFO • GPU • Core Input Power + SoC Input Power");
            }
        }

        private static T ReadStruct<T>(
            MemoryMappedViewAccessor accessor,
            long offset,
            int size)
            where T : struct
        {
            byte[] buffer = new byte[size];
            accessor.ReadArray(offset, buffer, 0, size);

            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.Copy(buffer, 0, ptr, size);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        private void ReadSensors(MemoryMappedViewAccessor accessor)
        {
            int sensorSize = Marshal.SizeOf<HwInfoSensorElement>();

            if (Header.SensorElementSize < sensorSize)
            {
                return;
            }

            for (uint index = 0; index < Header.SensorElementCount; index++)
            {
                long offset =
                    Header.SensorSectionOffset +
                    (index * Header.SensorElementSize);

                Sensors[index] =
                    ReadStruct<HwInfoSensorElement>(
                        accessor,
                        offset,
                        sensorSize);
            }
        }

        private void UpdateCpuSensor(HwInfoReadingElement reading)
        {
            if (!IsCpuReading(reading))
            {
                return;
            }

            if (ContainsLabel(reading, "Core 0 Clock (perf #1)"))
            {
                Data.CpuClock = reading.Value;
                SetSource("CpuClock", reading);
            }
            else if (ContainsLabel(reading, "CPU Package Power"))
            {
                Data.CpuPower = reading.Value;
                SetSource("CpuPower", reading);
            }
            else if (ContainsLabel(reading, "CPU VDDCR_VDD Voltage"))
            {
                Data.CpuTension = reading.Value;
                SetSource("CpuTension", reading);
            }
        }

        private void UpdateGpuSensor(
            HwInfoReadingElement reading,
            ref double gpuPowerCore,
            ref double gpuPowerSoc)
        {
            if (!IsGpuReading(reading))
            {
                return;
            }

            if (ContainsLabel(reading, "GPU Clock (Effective)"))
            {
                Data.GpuClock = reading.Value;
                SetSource("GpuClock", reading);
            }
            else if (ContainsLabel(reading, "GPU Temperature"))
            {
                Data.GpuTemperature = reading.Value;
                SetSource("GpuTemperature", reading);
            }
            else if (ContainsLabel(reading, "GPU Hot Spot"))
            {
                Data.GpuHotspot = reading.Value;
                SetSource("GpuHotspot", reading);
            }
            else if (ContainsLabel(reading, "GPU Memory Junction"))
            {
                Data.GpuMemoryJunction = reading.Value;
                SetSource("GpuMemoryJunction", reading);
            }
            else if (ContainsLabel(reading, "GPU Core Input Power"))
            {
                gpuPowerCore = reading.Value;
            }
            else if (ContainsLabel(reading, "GPU SoC Input Power"))
            {
                gpuPowerSoc = reading.Value;
            }
            else if (ContainsLabel(reading, "GPU Core Voltage"))
            {
                Data.GpuTension = reading.Value;
                SetSource("GpuTension", reading);
            }
        }

        private void UpdateMemorySensor(HwInfoReadingElement reading)
        {
            if (MatchesLabel(reading, "Memory Clock") ||
                MatchesLabel(reading, "DRAM Frequency"))
            {
                Data.RamClock = reading.Value;
                SetSource("RamClock", reading);
            }
        }

        private void UpdateFrameRateSensor(HwInfoReadingElement reading)
        {
            if (IsFrameRateReading(reading) &&
                (ContainsLabel(reading, "Framerate") ||
                ContainsLabel(reading, "Frame Rate") ||
                ContainsLabel(reading, "FPS")))
            {
                Data.Fps = reading.Value;
                SetSource("Fps", reading);
            }
        }

        private bool IsCpuReading(HwInfoReadingElement reading)
        {
            return IsReadingFrom(
                reading,
                name => name.Contains("CPU", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Ryzen", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Intel Core", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsGpuReading(HwInfoReadingElement reading)
        {
            return IsReadingFrom(
                reading,
                name => name.Contains("GPU", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Radeon", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("GeForce", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsMemoryReading(HwInfoReadingElement reading)
        {
            return IsReadingFrom(
                reading,
                name => name.Contains("Memory Timings", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("System Memory", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsFrameRateReading(HwInfoReadingElement reading)
        {
            return IsReadingFrom(
                reading,
                name => name.Contains("PresentMon", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("RTSS", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("Frame", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsReadingFrom(
            HwInfoReadingElement reading,
            Func<string, bool> predicate)
        {
            if (!Sensors.TryGetValue(reading.SensorIndex, out HwInfoSensorElement sensor))
            {
                return Sensors.Count == 0;
            }

            string sensorName =
                string.IsNullOrWhiteSpace(sensor.SensorNameUser)
                    ? sensor.SensorNameOrig ?? string.Empty
                    : sensor.SensorNameUser;

            return predicate(sensorName);
        }

        private void SetSource(
            string metricId,
            HwInfoReadingElement reading)
        {
            string sensorName = Sensors.TryGetValue(
                    reading.SensorIndex,
                    out HwInfoSensorElement sensor)
                ? string.IsNullOrWhiteSpace(sensor.SensorNameUser)
                    ? sensor.SensorNameOrig
                    : sensor.SensorNameUser
                : "capteur non groupé";

            Data.SetSource(
                metricId,
                $"HWiNFO • {sensorName} • {reading.LabelOrig}");
        }

        private static bool ContainsLabel(
            HwInfoReadingElement reading,
            string label)
        {
            return reading.LabelOrig?.Contains(
                label,
                StringComparison.OrdinalIgnoreCase) == true;
        }

        private static bool MatchesLabel(
            HwInfoReadingElement reading,
            string label)
        {
            return string.Equals(
                reading.LabelOrig?.Trim(),
                label,
                StringComparison.OrdinalIgnoreCase);
        }

        private void LogConnectError(Exception ex)
        {
            if (DateTime.Now - lastConnectErrorLog < TimeSpan.FromSeconds(10))
            {
                return;
            }

            lastConnectErrorLog = DateTime.Now;
            LoggerService.Log($"HWiNFO Connect Error: {ex.Message}");
        }
    }
}
