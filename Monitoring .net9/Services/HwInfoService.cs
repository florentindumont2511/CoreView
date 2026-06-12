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

        public HwInfoSharedMemHeader Header { get; private set; }

        public SensorData Data { get; } = new();

        public List<HwInfoReadingElement> Readings { get; } = [];

        public bool IsConnected { get; private set; }

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

                using var accessor = memoryFile.CreateViewAccessor();
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
            var cpuTempReading = Readings.FirstOrDefault(
                r => ContainsLabel(r, "Tctl/Tdie"));

            if (!string.IsNullOrEmpty(cpuTempReading.LabelOrig))
            {
                Data.CpuTemperature = cpuTempReading.Value;
            }
        }

        public void UpdateAdvancedSensors()
        {
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

        private void UpdateCpuSensor(HwInfoReadingElement reading)
        {
            if (ContainsLabel(reading, "Core 0 Clock (perf #1)"))
            {
                Data.CpuClock = reading.Value;
            }
            else if (ContainsLabel(reading, "CPU Package Power"))
            {
                Data.CpuPower = reading.Value;
            }
            else if (ContainsLabel(reading, "CPU VDDCR_VDD Voltage"))
            {
                Data.CpuTension = reading.Value;
            }
        }

        private void UpdateGpuSensor(
            HwInfoReadingElement reading,
            ref double gpuPowerCore,
            ref double gpuPowerSoc)
        {
            if (ContainsLabel(reading, "GPU Clock (Effective)"))
            {
                Data.GpuClock = reading.Value;
            }
            else if (ContainsLabel(reading, "GPU Temperature"))
            {
                Data.GpuTemperature = reading.Value;
            }
            else if (ContainsLabel(reading, "GPU Hot Spot"))
            {
                Data.GpuHotspot = reading.Value;
            }
            else if (ContainsLabel(reading, "GPU Memory Junction"))
            {
                Data.GpuMemoryJunction = reading.Value;
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
            }
        }

        private void UpdateMemorySensor(HwInfoReadingElement reading)
        {
            if (MatchesLabel(reading, "Memory Clock") ||
                MatchesLabel(reading, "DRAM Frequency"))
            {
                Data.RamClock = reading.Value;
            }
        }

        private void UpdateFrameRateSensor(HwInfoReadingElement reading)
        {
            if (ContainsLabel(reading, "Framerate") ||
                ContainsLabel(reading, "Frame Rate") ||
                ContainsLabel(reading, "FPS"))
            {
                Data.Fps = reading.Value;
            }
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
