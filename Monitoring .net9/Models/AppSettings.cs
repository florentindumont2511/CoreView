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

        public double DashboardScale { get; set; } = 1.0;

        public string Theme { get; set; } = "Dark";

        public bool ShowAdvancedSensors { get; set; } = true;

        public bool ShowMiniGraphs { get; set; } = true;
    }
}
