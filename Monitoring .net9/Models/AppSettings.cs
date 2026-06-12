namespace Monitoring_net9.Models
{
    public class AppSettings
    {
        public string SelectedScreen { get; set; } = "DISPLAY1";

        public bool Fullscreen { get; set; } = true;

        public string HwInfoPath { get; set; } =
            @"C:\Program Files\HWiNFO64\HWiNFO64.EXE";

        public double CpuWarningTemperature { get; set; } = 70;

        public double CpuDangerTemperature { get; set; } = 90;

        public double GpuWarningTemperature { get; set; } = 80;

        public double GpuDangerTemperature { get; set; } = 95;

        public double UsageWarningPercent { get; set; } = 90;

        public double UsageDangerPercent { get; set; } = 100;

        public double DashboardScale { get; set; } = 1.0;

        public string DashboardScalePreset { get; set; } = "Custom";

        public string Theme { get; set; } = "Dark";

        public string DateTimeLanguage { get; set; } = "French";

        public bool ShowAdvancedSensors { get; set; } = true;

        public bool ShowMiniGraphs { get; set; } = true;

        public bool ShowCpuGraph { get; set; } = true;

        public bool ShowGpuGraph { get; set; } = true;

        public bool ShowCpuUsageGraph { get; set; } = true;

        public bool ShowCpuTemperatureGraph { get; set; } = true;

        public bool ShowGpuUsageGraph { get; set; } = true;

        public bool ShowGpuTemperatureGraph { get; set; } = true;

        public List<string> HiddenSensors { get; set; } = [];

        public int HistoryDurationSeconds { get; set; } = 60;
    }
}
