using Monitoring_net9.Models;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using MediaBrush = System.Windows.Media.Brush;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;
using WpfPoint = System.Windows.Point;

namespace Monitoring_net9.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static readonly CultureInfo FrenchCulture = new("fr-FR");

        private string currentTime = string.Empty;
        private string currentDate = string.Empty;
        private string cpuUsage = "--";
        private string cpuTemperature = "--";
        private string cpuClock = "--";
        private string cpuPower = "--";
        private string cpuTension = "--";
        private string ramUsage = "--";
        private string gpuUsage = "--";
        private string gpuTemperature = "--";
        private string gpuMemory = "--";
        private string gpuClock = "--";
        private string gpuHotspot = "--";
        private string gpuMemoryJunction = "--";
        private string gpuPower = "--";
        private string gpuTension = "--";
        private string gpuTensionUnit = string.Empty;
        private double dashboardScale = 1.0;
        private double cpuWarningTemperature = 70;
        private double cpuDangerTemperature = 90;
        private double gpuWarningTemperature = 80;
        private double gpuDangerTemperature = 95;
        private Visibility advancedSensorsVisibility = Visibility.Visible;
        private Visibility miniGraphsVisibility = Visibility.Visible;
        private PointCollection cpuUsageHistoryPoints = [];
        private PointCollection cpuTemperatureHistoryPoints = [];
        private PointCollection gpuUsageHistoryPoints = [];
        private PointCollection gpuTemperatureHistoryPoints = [];
        private MediaBrush cpuTemperatureBrush = MediaBrushes.White;
        private MediaBrush gpuTemperatureBrush = MediaBrushes.White;
        private MediaBrush dashboardBrush =
            BrushFromRgb(21, 21, 21);
        private MediaBrush tileBrush =
            BrushFromRgb(37, 37, 37);
        private MediaBrush headerBrush = MediaBrushes.White;
        private MediaBrush secondaryTextBrush =
            BrushFromRgb(170, 170, 170);
        private MediaBrush titleBrush =
            BrushFromRgb(168, 168, 168);
        private MediaBrush unitBrush =
            BrushFromRgb(192, 192, 192);
        private MediaBrush chartLineBrush =
            BrushFromRgb(70, 190, 155);

        private readonly Queue<double> cpuUsageHistory = new();
        private readonly Queue<double> cpuTemperatureHistory = new();
        private readonly Queue<double> gpuUsageHistory = new();
        private readonly Queue<double> gpuTemperatureHistory = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string CurrentTime
        {
            get => currentTime;
            private set => SetProperty(ref currentTime, value);
        }

        public string CurrentDate
        {
            get => currentDate;
            private set => SetProperty(ref currentDate, value);
        }

        public string CpuUsage
        {
            get => cpuUsage;
            private set => SetProperty(ref cpuUsage, value);
        }

        public string CpuTemperature
        {
            get => cpuTemperature;
            private set => SetProperty(ref cpuTemperature, value);
        }

        public string CpuClock
        {
            get => cpuClock;
            private set => SetProperty(ref cpuClock, value);
        }

        public string CpuPower
        {
            get => cpuPower;
            private set => SetProperty(ref cpuPower, value);
        }

        public string CpuTension
        {
            get => cpuTension;
            private set => SetProperty(ref cpuTension, value);
        }

        public string RamUsage
        {
            get => ramUsage;
            private set => SetProperty(ref ramUsage, value);
        }

        public string GpuUsage
        {
            get => gpuUsage;
            private set => SetProperty(ref gpuUsage, value);
        }

        public string GpuTemperature
        {
            get => gpuTemperature;
            private set => SetProperty(ref gpuTemperature, value);
        }

        public string GpuMemory
        {
            get => gpuMemory;
            private set => SetProperty(ref gpuMemory, value);
        }

        public string GpuClock
        {
            get => gpuClock;
            private set => SetProperty(ref gpuClock, value);
        }

        public string GpuHotspot
        {
            get => gpuHotspot;
            private set => SetProperty(ref gpuHotspot, value);
        }

        public string GpuMemoryJunction
        {
            get => gpuMemoryJunction;
            private set => SetProperty(ref gpuMemoryJunction, value);
        }

        public string GpuPower
        {
            get => gpuPower;
            private set => SetProperty(ref gpuPower, value);
        }

        public string GpuTension
        {
            get => gpuTension;
            private set => SetProperty(ref gpuTension, value);
        }

        public string GpuTensionUnit
        {
            get => gpuTensionUnit;
            private set => SetProperty(ref gpuTensionUnit, value);
        }

        public MediaBrush CpuTemperatureBrush
        {
            get => cpuTemperatureBrush;
            private set => SetProperty(ref cpuTemperatureBrush, value);
        }

        public MediaBrush GpuTemperatureBrush
        {
            get => gpuTemperatureBrush;
            private set => SetProperty(ref gpuTemperatureBrush, value);
        }

        public double DashboardScale
        {
            get => dashboardScale;
            private set => SetProperty(ref dashboardScale, value);
        }

        public Visibility AdvancedSensorsVisibility
        {
            get => advancedSensorsVisibility;
            private set => SetProperty(ref advancedSensorsVisibility, value);
        }

        public Visibility MiniGraphsVisibility
        {
            get => miniGraphsVisibility;
            private set => SetProperty(ref miniGraphsVisibility, value);
        }

        public PointCollection CpuUsageHistoryPoints
        {
            get => cpuUsageHistoryPoints;
            private set => SetProperty(ref cpuUsageHistoryPoints, value);
        }

        public PointCollection CpuTemperatureHistoryPoints
        {
            get => cpuTemperatureHistoryPoints;
            private set => SetProperty(ref cpuTemperatureHistoryPoints, value);
        }

        public PointCollection GpuUsageHistoryPoints
        {
            get => gpuUsageHistoryPoints;
            private set => SetProperty(ref gpuUsageHistoryPoints, value);
        }

        public PointCollection GpuTemperatureHistoryPoints
        {
            get => gpuTemperatureHistoryPoints;
            private set => SetProperty(ref gpuTemperatureHistoryPoints, value);
        }

        public MediaBrush DashboardBrush
        {
            get => dashboardBrush;
            private set => SetProperty(ref dashboardBrush, value);
        }

        public MediaBrush TileBrush
        {
            get => tileBrush;
            private set => SetProperty(ref tileBrush, value);
        }

        public MediaBrush HeaderBrush
        {
            get => headerBrush;
            private set => SetProperty(ref headerBrush, value);
        }

        public MediaBrush SecondaryTextBrush
        {
            get => secondaryTextBrush;
            private set => SetProperty(ref secondaryTextBrush, value);
        }

        public MediaBrush TitleBrush
        {
            get => titleBrush;
            private set => SetProperty(ref titleBrush, value);
        }

        public MediaBrush UnitBrush
        {
            get => unitBrush;
            private set => SetProperty(ref unitBrush, value);
        }

        public MediaBrush ChartLineBrush
        {
            get => chartLineBrush;
            private set => SetProperty(ref chartLineBrush, value);
        }

        public void ApplySettings(AppSettings settings)
        {
            cpuWarningTemperature = settings.CpuWarningTemperature;
            cpuDangerTemperature = settings.CpuDangerTemperature;
            gpuWarningTemperature = settings.GpuWarningTemperature;
            gpuDangerTemperature = settings.GpuDangerTemperature;

            DashboardScale = Math.Clamp(settings.DashboardScale, 0.75, 1.35);
            AdvancedSensorsVisibility =
                settings.ShowAdvancedSensors ? Visibility.Visible : Visibility.Collapsed;
            MiniGraphsVisibility =
                settings.ShowMiniGraphs ? Visibility.Visible : Visibility.Collapsed;

            ApplyTheme(settings.Theme);
        }

        public void UpdateClock(DateTime now)
        {
            CurrentTime = now.ToString("HH:mm:ss");
            CurrentDate = now.ToString("dddd dd MMMM yyyy", FrenchCulture);
        }

        public void UpdateSensors(SensorData data)
        {
            CpuUsage = FormatRequired(data.CpuUsage, "F1");
            CpuTemperature = FormatOptional(data.CpuTemperature, "F1");
            CpuClock = FormatOptional(data.CpuClock / 1000, "F2");
            CpuPower = FormatOptional(data.CpuPower, "F1");
            CpuTension = FormatOptional(data.CpuTension, "F3");
            RamUsage = FormatRequired(data.RamUsed, "F1");

            GpuUsage = FormatRequired(data.GpuUsage, "F1");
            GpuTemperature = FormatOptional(data.GpuTemperature, "F0");
            GpuMemory = FormatRequired(data.GpuMemoryUsedGB, "F1");
            GpuClock = FormatOptional(data.GpuClock, "F0");
            GpuHotspot = FormatOptional(data.GpuHotspot, "F1");
            GpuMemoryJunction = FormatOptional(data.GpuMemoryJunction, "F0");
            GpuPower = FormatOptional(data.GpuPower, "F1");

            UpdateGpuTension(data.GpuTension);

            CpuTemperatureBrush =
                GetTemperatureBrush(
                    data.CpuTemperature,
                    cpuWarningTemperature,
                    cpuDangerTemperature);
            GpuTemperatureBrush =
                GetTemperatureBrush(
                    data.GpuTemperature,
                    gpuWarningTemperature,
                    gpuDangerTemperature);

            UpdateHistory(data);
        }

        private static string FormatRequired(double value, string format)
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        private static string FormatOptional(double value, string format)
        {
            if (value <= 0)
            {
                return "--";
            }

            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        private void UpdateGpuTension(double tension)
        {
            if (tension <= 0)
            {
                GpuTension = "--";
                GpuTensionUnit = string.Empty;
                return;
            }

            if (tension >= 1)
            {
                GpuTension = tension.ToString("F3", CultureInfo.InvariantCulture);
                GpuTensionUnit = "V";
                return;
            }

            GpuTension = (tension * 1000).ToString("F0", CultureInfo.InvariantCulture);
            GpuTensionUnit = "mV";
        }

        private static MediaBrush GetTemperatureBrush(
            double temperature,
            double warningThreshold,
            double dangerThreshold)
        {
            if (temperature <= 0)
            {
                return MediaBrushes.White;
            }

            if (temperature > dangerThreshold)
            {
                return MediaBrushes.Red;
            }

            if (temperature > warningThreshold)
            {
                return MediaBrushes.Orange;
            }

            return MediaBrushes.White;
        }

        private void UpdateHistory(SensorData data)
        {
            AddHistoryValue(cpuUsageHistory, data.CpuUsage);
            AddHistoryValue(cpuTemperatureHistory, data.CpuTemperature);
            AddHistoryValue(gpuUsageHistory, data.GpuUsage);
            AddHistoryValue(gpuTemperatureHistory, data.GpuTemperature);

            CpuUsageHistoryPoints =
                CreatePoints(cpuUsageHistory, 100);
            CpuTemperatureHistoryPoints =
                CreatePoints(cpuTemperatureHistory, cpuDangerTemperature);
            GpuUsageHistoryPoints =
                CreatePoints(gpuUsageHistory, 100);
            GpuTemperatureHistoryPoints =
                CreatePoints(gpuTemperatureHistory, gpuDangerTemperature);
        }

        private static void AddHistoryValue(
            Queue<double> history,
            double value)
        {
            history.Enqueue(Math.Max(0, value));

            while (history.Count > 60)
            {
                history.Dequeue();
            }
        }

        private static PointCollection CreatePoints(
            IReadOnlyCollection<double> values,
            double maxValue)
        {
            const double width = 180;
            const double height = 48;

            var points = new PointCollection();

            if (values.Count == 0)
            {
                return points;
            }

            int index = 0;
            double step = values.Count == 1
                ? width
                : width / (values.Count - 1);

            foreach (double value in values)
            {
                double ratio = maxValue <= 0
                    ? 0
                    : Math.Clamp(value / maxValue, 0, 1);

                points.Add(
                    new WpfPoint(
                        index * step,
                        height - (ratio * height)));

                index++;
            }

            return points;
        }

        private void ApplyTheme(string theme)
        {
            if (string.Equals(theme, "Contrast", StringComparison.OrdinalIgnoreCase))
            {
                DashboardBrush = MediaBrushes.Black;
                TileBrush = BrushFromRgb(18, 18, 18);
                HeaderBrush = MediaBrushes.White;
                SecondaryTextBrush = BrushFromRgb(220, 220, 220);
                TitleBrush = MediaBrushes.White;
                UnitBrush = BrushFromRgb(230, 230, 230);
                ChartLineBrush = BrushFromRgb(255, 210, 72);
                return;
            }

            DashboardBrush = BrushFromRgb(21, 21, 21);
            TileBrush = BrushFromRgb(37, 37, 37);
            HeaderBrush = MediaBrushes.White;
            SecondaryTextBrush = BrushFromRgb(170, 170, 170);
            TitleBrush = BrushFromRgb(168, 168, 168);
            UnitBrush = BrushFromRgb(192, 192, 192);
            ChartLineBrush = BrushFromRgb(70, 190, 155);
        }

        private static MediaBrush BrushFromRgb(
            byte red,
            byte green,
            byte blue)
        {
            var brush =
                new SolidColorBrush(MediaColor.FromRgb(red, green, blue));

            brush.Freeze();

            return brush;
        }

        private void SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
