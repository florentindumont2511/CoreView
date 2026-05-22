namespace Monitoring_net9.Models
{
    public class SensorData
    {
        // CPU
        public double CpuUsage { get; set; }

        public double CpuTemperature { get; set; }

        public double CpuClock { get; set; }

        public double CpuPower { get; set; }

        public double CpuTension { get; set; }

        // RAM
        public double RamUsed { get; set; }

        // GPU
        public double GpuUsage { get; set; }

        public double GpuTemperature { get; set; }

        public double GpuMemoryUsedGB { get; set; }

        public double GpuClock { get; set; }

        public double GpuHotspot { get; set; }

        public double GpuMemoryJunction { get; set; }

        public double GpuPower { get; set; }

        public double GpuTension { get; set; }
    }
}