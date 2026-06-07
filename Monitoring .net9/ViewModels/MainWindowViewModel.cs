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
        private string cpuName = "CPU";
        private string gpuName = "GPU";
        private string cpuUsage = "--";
        private string cpuTemperature = "--";
        private string cpuClock = "--";
        private string cpuPower = "--";
        private string cpuTension = "--";
        private string ramUsage = "--";
        private string ramTotal = "--";
        private string ramUsagePercent = "--";
        private string ramClock = "--";
        private string gpuUsage = "--";
        private string gpuTemperature = "--";
        private string gpuMemory = "--";
        private string gpuMemoryTotal = "--";
        private string gpuMemoryUsagePercent = "--";
        private string gpuClock = "--";
        private string gpuHotspot = "--";
        private string gpuMemoryJunction = "--";
        private string gpuPower = "--";
        private string gpuTension = "--";
        private string gpuTensionUnit = string.Empty;
        private string fps = "--";
        private string totalPower = "--";
        private string historyDurationLabel = "60s";
        private string hwInfoStatus = "HWiNFO inconnu";
        private string cpuTemperatureAxisLabel = "90°C";
        private string gpuTemperatureAxisLabel = "95°C";
        private double dashboardScale = 1.0;
        private double cpuWarningTemperature = 70;
        private double cpuDangerTemperature = 90;
        private double gpuWarningTemperature = 80;
        private double gpuDangerTemperature = 95;
        private int historyDurationSeconds = 60;
        private Visibility advancedSensorsVisibility = Visibility.Visible;
        private Visibility miniGraphsVisibility = Visibility.Visible;
        private PointCollection cpuUsageHistoryPoints = [];
        private PointCollection cpuUsageAreaPoints = [];
        private PointCollection cpuTemperatureHistoryPoints = [];
        private PointCollection gpuUsageHistoryPoints = [];
        private PointCollection gpuUsageAreaPoints = [];
        private PointCollection gpuTemperatureHistoryPoints = [];
        private MediaBrush cpuTemperatureBrush = MediaBrushes.White;
        private MediaBrush gpuTemperatureBrush = MediaBrushes.White;
        private MediaBrush hwInfoStatusBrush = BrushFromRgb(170, 170, 170);
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
        private MediaBrush chartAreaBrush =
            BrushFromArgb(64, 70, 190, 155);
        private MediaBrush chartGridBrush =
            BrushFromArgb(55, 255, 255, 255);

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

        public string CpuName
        {
            get => cpuName;
            private set => SetProperty(ref cpuName, value);
        }

        public string GpuName
        {
            get => gpuName;
            private set => SetProperty(ref gpuName, value);
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

        public string RamTotal
        {
            get => ramTotal;
            private set => SetProperty(ref ramTotal, value);
        }

        public string RamUsagePercent
        {
            get => ramUsagePercent;
            private set => SetProperty(ref ramUsagePercent, value);
        }

        public string RamClock
        {
            get => ramClock;
            private set => SetProperty(ref ramClock, value);
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

        public string GpuMemoryTotal
        {
            get => gpuMemoryTotal;
            private set => SetProperty(ref gpuMemoryTotal, value);
        }

        public string GpuMemoryUsagePercent
        {
            get => gpuMemoryUsagePercent;
            private set => SetProperty(ref gpuMemoryUsagePercent, value);
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

        public string Fps
        {
            get => fps;
            private set => SetProperty(ref fps, value);
        }

        public string TotalPower
        {
            get => totalPower;
            private set => SetProperty(ref totalPower, value);
        }

        public string HistoryDurationLabel
        {
            get => historyDurationLabel;
            private set => SetProperty(ref historyDurationLabel, value);
        }

        public string HwInfoStatus
        {
            get => hwInfoStatus;
            private set => SetProperty(ref hwInfoStatus, value);
        }

        public string CpuTemperatureAxisLabel
        {
            get => cpuTemperatureAxisLabel;
            private set => SetProperty(ref cpuTemperatureAxisLabel, value);
        }

        public string GpuTemperatureAxisLabel
        {
            get => gpuTemperatureAxisLabel;
            private set => SetProperty(ref gpuTemperatureAxisLabel, value);
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

        public MediaBrush HwInfoStatusBrush
        {
            get => hwInfoStatusBrush;
            private set => SetProperty(ref hwInfoStatusBrush, value);
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

        public PointCollection CpuUsageAreaPoints
        {
            get => cpuUsageAreaPoints;
            private set => SetProperty(ref cpuUsageAreaPoints, value);
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

        public PointCollection GpuUsageAreaPoints
        {
            get => gpuUsageAreaPoints;
            private set => SetProperty(ref gpuUsageAreaPoints, value);
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

        public MediaBrush ChartAreaBrush
        {
            get => chartAreaBrush;
            private set => SetProperty(ref chartAreaBrush, value);
        }

        public MediaBrush ChartGridBrush
        {
            get => chartGridBrush;
            private set => SetProperty(ref chartGridBrush, value);
        }

        public void ApplySettings(AppSettings settings)
        {
            cpuWarningTemperature =
                GetFiniteOrDefault(settings.CpuWarningTemperature, 70);
            cpuDangerTemperature =
                GetFiniteOrDefault(settings.CpuDangerTemperature, 90);
            gpuWarningTemperature =
                GetFiniteOrDefault(settings.GpuWarningTemperature, 80);
            gpuDangerTemperature =
                GetFiniteOrDefault(settings.GpuDangerTemperature, 95);
            historyDurationSeconds =
                Math.Clamp(settings.HistoryDurationSeconds, 30, 300);
            HistoryDurationLabel =
                historyDurationSeconds < 60
                    ? $"{historyDurationSeconds}s"
                    : $"{historyDurationSeconds / 60}min";
            CpuTemperatureAxisLabel =
                $"{cpuDangerTemperature:F0}°C";
            GpuTemperatureAxisLabel =
                $"{gpuDangerTemperature:F0}°C";

            DashboardScale =
                Math.Clamp(GetFiniteOrDefault(settings.DashboardScale, 1.0), 0.75, 1.35);
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

        public void UpdateHwInfoStatus(bool isConnected)
        {
            if (isConnected)
            {
                HwInfoStatus = "HWiNFO connecté";
                HwInfoStatusBrush = BrushFromRgb(70, 190, 155);
                return;
            }

            HwInfoStatus = "HWiNFO déconnecté";
            HwInfoStatusBrush = MediaBrushes.Orange;
        }

        public void UpdateSensors(SensorData data)
        {
            CpuName = FormatHardwareName(data.CpuName, "CPU");
            GpuName = FormatHardwareName(data.GpuName, "GPU");
            CpuUsage = FormatRequired(data.CpuUsage, "F1");
            CpuTemperature = FormatOptional(data.CpuTemperature, "F1");
            CpuClock = FormatOptional(data.CpuClock / 1000, "F2");
            CpuPower = FormatOptional(data.CpuPower, "F1");
            CpuTension = FormatOptional(data.CpuTension, "F3");
            RamUsage = FormatRequired(data.RamUsed, "F1");
            RamTotal = FormatOptional(data.RamTotal, "F1");
            RamUsagePercent = FormatOptional(data.RamUsagePercent, "F0");
            RamClock = FormatOptional(data.RamClock, "F0");

            GpuUsage = FormatRequired(data.GpuUsage, "F1");
            GpuTemperature = FormatOptional(data.GpuTemperature, "F0");
            GpuMemory = FormatRequired(data.GpuMemoryUsedGB, "F1");
            GpuMemoryTotal = FormatOptional(data.GpuMemoryTotalGB, "F1");
            GpuMemoryUsagePercent =
                FormatOptional(
                    CalculatePercent(
                        data.GpuMemoryUsedGB,
                        data.GpuMemoryTotalGB,
                        data.GpuMemoryUsagePercent),
                    "F0");
            GpuClock = FormatOptional(data.GpuClock, "F0");
            GpuHotspot = FormatOptional(data.GpuHotspot, "F1");
            GpuMemoryJunction = FormatOptional(data.GpuMemoryJunction, "F0");
            GpuPower = FormatOptional(data.GpuPower, "F1");
            Fps = FormatOptional(data.Fps, "F0");
            TotalPower = FormatOptional(data.TotalPower, "F1");

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
            if (!IsFinite(value))
            {
                return "--";
            }

            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        private static string FormatHardwareName(
            string name,
            string fallback)
        {
            return string.IsNullOrWhiteSpace(name)
                ? fallback
                : name.Trim();
        }

        private static string FormatOptional(double value, string format)
        {
            if (!IsFinite(value) || value <= 0)
            {
                return "--";
            }

            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        private static double CalculatePercent(
            double used,
            double total,
            double fallback)
        {
            if (IsFinite(used) &&
                IsFinite(total) &&
                used > 0 &&
                total > 0)
            {
                return Math.Clamp((used / total) * 100, 0, 100);
            }

            return fallback;
        }

        private void UpdateGpuTension(double tension)
        {
            if (!IsFinite(tension) || tension <= 0)
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
            if (!IsFinite(temperature) || temperature <= 0)
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
                CreateUsagePoints(cpuUsageHistory);
            CpuUsageAreaPoints =
                CreateAreaPoints(CpuUsageHistoryPoints);
            CpuTemperatureHistoryPoints =
                CreatePoints(cpuTemperatureHistory, cpuDangerTemperature);
            GpuUsageHistoryPoints =
                CreateUsagePoints(gpuUsageHistory);
            GpuUsageAreaPoints =
                CreateAreaPoints(GpuUsageHistoryPoints);
            GpuTemperatureHistoryPoints =
                CreatePoints(gpuTemperatureHistory, gpuDangerTemperature);
        }

        private void AddHistoryValue(
            Queue<double> history,
            double value)
        {
            if (!IsFinite(value))
            {
                value = 0;
            }

            history.Enqueue(Math.Max(0, value));

            while (history.Count > historyDurationSeconds)
            {
                history.Dequeue();
            }
        }

        private static PointCollection CreatePoints(
            IReadOnlyCollection<double> values,
            double maxValue)
        {
            const double width = 320;
            const double height = 170;
            const double topPadding = 6;
            const double bottomPadding = 6;
            const double graphHeight = height - topPadding - bottomPadding;

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
                        topPadding + ((1 - ratio) * graphHeight)));

                index++;
            }

            return points;
        }

        private static PointCollection CreateUsagePoints(
            IReadOnlyCollection<double> values)
        {
            double maxValue =
                Math.Max(25, values.Count == 0 ? 100 : values.Where(IsFinite).DefaultIfEmpty(0).Max());

            return CreatePoints(values, maxValue);
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static double GetFiniteOrDefault(
            double value,
            double fallback)
        {
            return IsFinite(value) ? value : fallback;
        }

        private static PointCollection CreateAreaPoints(
            PointCollection linePoints)
        {
            const double height = 170;
            const double baseline = height - 6;

            var points = new PointCollection();

            if (linePoints.Count == 0)
            {
                return points;
            }

            points.Add(
                new WpfPoint(linePoints[0].X, baseline));

            foreach (WpfPoint point in linePoints)
            {
                points.Add(point);
            }

            points.Add(
                new WpfPoint(linePoints[^1].X, baseline));

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
                ChartAreaBrush = BrushFromArgb(72, 255, 210, 72);
                ChartGridBrush = BrushFromArgb(75, 255, 255, 255);
                return;
            }

            DashboardBrush = BrushFromRgb(21, 21, 21);
            TileBrush = BrushFromRgb(37, 37, 37);
            HeaderBrush = MediaBrushes.White;
            SecondaryTextBrush = BrushFromRgb(170, 170, 170);
            TitleBrush = BrushFromRgb(168, 168, 168);
            UnitBrush = BrushFromRgb(192, 192, 192);
            ChartLineBrush = BrushFromRgb(70, 190, 155);
            ChartAreaBrush = BrushFromArgb(64, 70, 190, 155);
            ChartGridBrush = BrushFromArgb(55, 255, 255, 255);
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

        private static MediaBrush BrushFromArgb(
            byte alpha,
            byte red,
            byte green,
            byte blue)
        {
            var brush =
                new SolidColorBrush(MediaColor.FromArgb(alpha, red, green, blue));

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
