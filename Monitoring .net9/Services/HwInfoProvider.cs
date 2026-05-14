using System.IO.MemoryMappedFiles;
using Monitoring_net9.Models;
using System.Runtime.InteropServices;
using System.Linq;

namespace Monitoring_net9.Services
{
    public class HwInfoService
    {
        private MemoryMappedFile? memoryFile;

        public HwInfoSharedMemHeader Header { get; private set; }

        public List<HwInfoReadingElement> Readings { get; private set; } = [];

        public double CpuTemperature { get; private set; }

        public bool Connect()
        {
            try
            {
                memoryFile = MemoryMappedFile.OpenExisting("Global\\HWiNFO_SENS_SM2");

                return true;
            }
            catch
            {
                return false;
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
            catch
            {
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
            catch
            {
                return false;
            }
        }

        public void UpdateCpuTemperature()
        {
            var cpuTempReading = Readings.FirstOrDefault(
                r => r.LabelOrig.Contains("Tctl/Tdie"));

            if (!string.IsNullOrEmpty(cpuTempReading.LabelOrig))
            {
                CpuTemperature = cpuTempReading.Value;
            }
        }
    }
}