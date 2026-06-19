namespace Monitoring_net9.Models
{
    public class SensorData
    {
        public Dictionary<string, string> SourceDetails { get; } = [];

        // CPU
        public string CpuName { get; set; } = string.Empty;

        public double CpuUsage { get; set; }

        public double CpuTemperature { get; set; }

        public double CpuClock { get; set; }

        public double CpuPower { get; set; }

        public double CpuTension { get; set; }

        // RAM
        public double RamUsed { get; set; }

        public double RamTotal { get; set; }

        public double RamUsagePercent { get; set; }

        public double RamClock { get; set; }

        // GPU
        public string GpuName { get; set; } = string.Empty;

        public double GpuUsage { get; set; }

        public double GpuTemperature { get; set; }

        public double GpuMemoryUsedGB { get; set; }

        public double GpuMemoryTotalGB { get; set; }

        public double GpuMemoryUsagePercent { get; set; }

        public double GpuClock { get; set; }

        public double GpuHotspot { get; set; }

        public double GpuMemoryJunction { get; set; }

        public double GpuPower { get; set; }

        public double GpuTension { get; set; }

        public double Fps { get; set; }

        public double TotalPower { get; set; }

        public void SetSource(string metricId, string source)
        {
            SourceDetails[metricId] = source;
        }

        public SensorData Copy()
        {
            var copy = new SensorData
            {
                CpuName = CpuName,
                CpuUsage = CpuUsage,
                CpuTemperature = CpuTemperature,
                CpuClock = CpuClock,
                CpuPower = CpuPower,
                CpuTension = CpuTension,
                RamUsed = RamUsed,
                RamTotal = RamTotal,
                RamUsagePercent = RamUsagePercent,
                RamClock = RamClock,
                GpuName = GpuName,
                GpuUsage = GpuUsage,
                GpuTemperature = GpuTemperature,
                GpuMemoryUsedGB = GpuMemoryUsedGB,
                GpuMemoryTotalGB = GpuMemoryTotalGB,
                GpuMemoryUsagePercent = GpuMemoryUsagePercent,
                GpuClock = GpuClock,
                GpuHotspot = GpuHotspot,
                GpuMemoryJunction = GpuMemoryJunction,
                GpuPower = GpuPower,
                GpuTension = GpuTension,
                Fps = Fps,
                TotalPower = TotalPower
            };

            foreach ((string metricId, string source) in SourceDetails)
            {
                copy.SourceDetails[metricId] = source;
            }

            return copy;
        }
    }
}
