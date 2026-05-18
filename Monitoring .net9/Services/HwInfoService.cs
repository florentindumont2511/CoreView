using System.IO.MemoryMappedFiles;
using Monitoring_net9.Models;
using System.Runtime.InteropServices;

namespace Monitoring_net9.Services
{
    public class HwInfoService
    {
        private MemoryMappedFile? memoryFile;

        public HwInfoSharedMemHeader Header { get; private set; }

        public SensorData Data { get; private set; } =  new SensorData();

        public List<HwInfoReadingElement> Readings { get; private set; } = [];

        public double GPUPower_Core { get; private set; }

        public double GPUPower_SoC { get; private set; }

        public bool IsConnected { get; private set; }


        public bool Connect()
        {
            try
            {
                // Déjà connecté
                if (memoryFile != null)
                {
                    return true;
                }

                memoryFile =
                    MemoryMappedFile.OpenExisting(
                        "Global\\HWiNFO_SENS_SM2");

                if (memoryFile == null)
                {
                    IsConnected = false;

                    return false;
                }

                IsConnected = true;

                LoggerService.Log(
                    "HWiNFO connected");

                return true;
            }
            catch (Exception ex)
            {
                LoggerService.Log(
                    $"HWiNFO Connect Error: {ex.Message}");

                IsConnected = false;

                return false;
            }
        }

        public void TryReconnect()
        {
            if (Connect())
            {
                ReadHeader();

                LoggerService.Log(
                    "HWiNFO reconnected");
            }
        }

        public void Disconnect()
        {
            try
            {
                memoryFile?.Dispose();

                memoryFile = null;

                IsConnected = false;

                LoggerService.Log(
                    "HWiNFO disconnected");
            }
            catch (Exception ex)
            {
                LoggerService.Log(
                    $"Disconnect Error: {ex.Message}");
            }
        }

        public bool ReadHeader()
        {
            try
            {
                using var accessor = memoryFile!.CreateViewAccessor();

                int size = Marshal.SizeOf<HwInfoSharedMemHeader>();

                byte[] buffer = new byte[size];

                accessor.ReadArray(0, buffer, 0, size);

                IntPtr ptr = Marshal.AllocHGlobal(size);

                Marshal.Copy(buffer, 0, ptr, size);

                Header = Marshal.PtrToStructure<HwInfoSharedMemHeader>(ptr);

                Marshal.FreeHGlobal(ptr);

                return true;
            }
            catch (Exception ex)
            {
                LoggerService.Log(
                    $"Error Reading Shared Memory: {ex.Message}");

                return false;
            }
        }

        public bool ReadReadings()
        {
            try
            {
                Readings.Clear();

                using var accessor = memoryFile!.CreateViewAccessor();

                int elementSize =
                    Marshal.SizeOf<HwInfoReadingElement>();

                for (int i = 0;
                     i < Header.ReadingElementCount;
                     i++)
                {
                    long offset =
                        Header.ReadingSectionOffset +
                        (i * Header.ReadingElementSize);

                    byte[] buffer = new byte[elementSize];

                    accessor.ReadArray(offset, buffer, 0, elementSize);

                    IntPtr ptr = Marshal.AllocHGlobal(elementSize);

                    Marshal.Copy(buffer, 0, ptr, elementSize);

                    var reading =
                        Marshal.PtrToStructure<HwInfoReadingElement>(ptr);

                    Marshal.FreeHGlobal(ptr);

                    Readings.Add(reading);
                }

                return true;
            }
            catch (Exception ex)
            {
                LoggerService.Log(
                    $"Error Reading Sensor: {ex.Message}");

                return false;
            }
        }

        public void UpdateCpuTemperature()
        {
            var cpuTempReading = Readings.FirstOrDefault(
                r => r.LabelOrig.Contains("Tctl/Tdie"));

            if (!string.IsNullOrEmpty(cpuTempReading.LabelOrig))
            {
                Data.CpuTemperature = cpuTempReading.Value;
            }
        }

        public void UpdateAdvancedSensors()
        {
            GPUPower_Core = 0;
            GPUPower_SoC = 0;

            foreach (var reading in Readings)
            {
                // CPU CLOCK
                if (reading.LabelOrig.Contains(
                    "Core 0 Clock (perf #1)") && reading.Value > 0) // Contrôle à chaque fois du nom de la sonde et qu'il n'y ait pas une valeur nulle
                {
                    Data.CpuClock = reading.Value;
                }

                // CPU POWER
                if (reading.LabelOrig.Contains(
                    "CPU Package Power") && reading.Value > 0)
                {
                    Data.CpuPower = reading.Value;
                }

                // GPU CLOCK
                if (reading.LabelOrig.Contains(
                    "GPU Clock (Effective)") && reading.Value > 0)
                {
                    Data.GpuClock = reading.Value;
                }

                // GPU Temp
                if (reading.LabelOrig.Contains(
                    "GPU Temperature") && reading.Value > 0)
                {
                    Data.GpuTemperature = reading.Value;
                }

                // GPU HOTSPOT
                if (reading.LabelOrig.Contains(
                    "GPU Hot Spot") && reading.Value > 0)
                {
                    Data.GpuHotspot = reading.Value;
                }

                // GPU MEMORY JUNCTION
                if (reading.LabelOrig.Contains(
                    "GPU Memory Junction") && reading.Value > 0)
                {
                    Data.GpuMemoryJunction = reading.Value;
                }

                // GPU Power
                if (reading.LabelOrig.Contains(
                    "GPU Core Input Power") && reading.Value > 0)
                {
                    GPUPower_Core = reading.Value;
                }

                if (reading.LabelOrig.Contains(
                    "GPU SoC Input Power") && reading.Value > 0)
                {
                    GPUPower_SoC = reading.Value;
                }
            }

            Data.GpuPower = GPUPower_Core + GPUPower_SoC;
        }
    }
}