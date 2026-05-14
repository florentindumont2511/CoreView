namespace Monitoring_net9.Models
{
    public class SensorData
    {
        // CPU
        public double CpuUsage { get; set; }

        public double CpuTemperature { get; set; }

        // RAM
        public double RamUsed { get; set; }

        // GPU
        public double GpuUsage { get; set; }

        public double GpuTemperature { get; set; }

        public double GpuMemoryUsedGB { get; set; }

    }
}