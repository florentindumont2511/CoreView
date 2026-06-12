namespace Monitoring_net9.Models
{
    public sealed record SensorOption(
        string Id,
        string DisplayName,
        bool IsAdvanced = false);

    public static class SensorOptionDefinitions
    {
        public static IReadOnlyList<SensorOption> All { get; } =
        [
            new("CpuUsage", "Utilisation CPU"),
            new("CpuTemperature", "Température CPU"),
            new("RamUsed", "RAM utilisée"),
            new("CpuClock", "Fréquence CPU"),
            new("CpuPower", "Puissance CPU", true),
            new("CpuTension", "Tension CPU", true),
            new("RamUsagePercent", "Utilisation RAM (%)"),
            new("RamTotal", "RAM totale"),
            new("RamClock", "Fréquence RAM", true),
            new("GpuUsage", "Utilisation GPU"),
            new("GpuTemperature", "Température GPU"),
            new("GpuMemoryUsed", "VRAM utilisée"),
            new("GpuClock", "Fréquence GPU"),
            new("GpuPower", "Puissance GPU", true),
            new("GpuTension", "Tension GPU", true),
            new("GpuHotspot", "Hotspot GPU", true),
            new("TotalPower", "Puissance CPU + GPU", true),
            new("GpuMemoryJunction", "Température VRAM", true),
            new("GpuMemoryUsagePercent", "Utilisation VRAM (%)"),
            new("GpuMemoryTotal", "VRAM totale"),
            new("Fps", "FPS")
        ];
    }
}
