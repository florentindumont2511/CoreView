using Monitoring_net9.Models;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using MediaBrush = System.Windows.Media.Brush;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;
using MediaGeometry = System.Windows.Media.Geometry;
using WpfPoint = System.Windows.Point;

namespace Monitoring_net9.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
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
        private double usageWarningPercent = 90;
        private double usageDangerPercent = 100;
        private int historyDurationSeconds = 60;
        private CultureInfo dateTimeCulture = new("fr-FR");
        private string timeFormat = "HH:mm:ss";
        private Visibility advancedSensorsVisibility = Visibility.Visible;
        private Visibility miniGraphsVisibility = Visibility.Visible;
        private Visibility cpuGraphVisibility = Visibility.Visible;
        private Visibility gpuGraphVisibility = Visibility.Visible;
        private Visibility cpuUsageGraphVisibility = Visibility.Visible;
        private Visibility cpuTemperatureGraphVisibility = Visibility.Visible;
        private Visibility gpuUsageGraphVisibility = Visibility.Visible;
        private Visibility gpuTemperatureGraphVisibility = Visibility.Visible;
        private int cpuUsageGraphColumnSpan = 1;
        private int cpuTemperatureGraphColumn = 2;
        private int cpuTemperatureGraphColumnSpan = 1;
        private int gpuUsageGraphColumnSpan = 1;
        private int gpuTemperatureGraphColumn = 2;
        private int gpuTemperatureGraphColumnSpan = 1;
        private IReadOnlyDictionary<string, Visibility> sensorVisibilities =
            SensorOptionDefinitions.All.ToDictionary(
                option => option.Id,
                _ => Visibility.Visible);
        private IReadOnlyDictionary<string, string> sensorStatistics =
            SensorOptionDefinitions.All.ToDictionary(
                option => option.Id,
                _ => "Min --  Moy --  Max --");
        private PointCollection cpuUsageHistoryPoints = [];
        private PointCollection cpuUsageAreaPoints = [];
        private PointCollection cpuTemperatureHistoryPoints = [];
        private PointCollection gpuUsageHistoryPoints = [];
        private PointCollection gpuUsageAreaPoints = [];
        private PointCollection gpuTemperatureHistoryPoints = [];
        private MediaGeometry cpuUsageHistoryGeometry = MediaGeometry.Empty;
        private MediaGeometry cpuUsageAreaGeometry = MediaGeometry.Empty;
        private MediaGeometry cpuTemperatureHistoryGeometry = MediaGeometry.Empty;
        private MediaGeometry gpuUsageHistoryGeometry = MediaGeometry.Empty;
        private MediaGeometry gpuUsageAreaGeometry = MediaGeometry.Empty;
        private MediaGeometry gpuTemperatureHistoryGeometry = MediaGeometry.Empty;
        private MediaBrush cpuTemperatureBrush = MediaBrushes.White;
        private MediaBrush gpuTemperatureBrush = MediaBrushes.White;
        private MediaBrush cpuUsageBrush = MediaBrushes.White;
        private MediaBrush gpuUsageBrush = MediaBrushes.White;
        private MediaBrush cpuUsageAreaBrush = BrushFromArgb(48, 255, 255, 255);
        private MediaBrush gpuUsageAreaBrush = BrushFromArgb(48, 255, 255, 255);
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
        private readonly Dictionary<string, RunningStatistics> runningStatistics = [];

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

        public MediaBrush CpuUsageBrush
        {
            get => cpuUsageBrush;
            private set => SetProperty(ref cpuUsageBrush, value);
        }

        public MediaBrush GpuUsageBrush
        {
            get => gpuUsageBrush;
            private set => SetProperty(ref gpuUsageBrush, value);
        }

        public MediaBrush CpuUsageAreaBrush
        {
            get => cpuUsageAreaBrush;
            private set => SetProperty(ref cpuUsageAreaBrush, value);
        }

        public MediaBrush GpuUsageAreaBrush
        {
            get => gpuUsageAreaBrush;
            private set => SetProperty(ref gpuUsageAreaBrush, value);
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

        public Visibility CpuGraphVisibility
        {
            get => cpuGraphVisibility;
            private set => SetProperty(ref cpuGraphVisibility, value);
        }

        public Visibility GpuGraphVisibility
        {
            get => gpuGraphVisibility;
            private set => SetProperty(ref gpuGraphVisibility, value);
        }

        public Visibility CpuUsageGraphVisibility
        {
            get => cpuUsageGraphVisibility;
            private set => SetProperty(ref cpuUsageGraphVisibility, value);
        }

        public Visibility CpuTemperatureGraphVisibility
        {
            get => cpuTemperatureGraphVisibility;
            private set => SetProperty(ref cpuTemperatureGraphVisibility, value);
        }

        public Visibility GpuUsageGraphVisibility
        {
            get => gpuUsageGraphVisibility;
            private set => SetProperty(ref gpuUsageGraphVisibility, value);
        }

        public Visibility GpuTemperatureGraphVisibility
        {
            get => gpuTemperatureGraphVisibility;
            private set => SetProperty(ref gpuTemperatureGraphVisibility, value);
        }

        public int CpuUsageGraphColumnSpan
        {
            get => cpuUsageGraphColumnSpan;
            private set => SetProperty(ref cpuUsageGraphColumnSpan, value);
        }

        public int CpuTemperatureGraphColumn
        {
            get => cpuTemperatureGraphColumn;
            private set => SetProperty(ref cpuTemperatureGraphColumn, value);
        }

        public int CpuTemperatureGraphColumnSpan
        {
            get => cpuTemperatureGraphColumnSpan;
            private set => SetProperty(ref cpuTemperatureGraphColumnSpan, value);
        }

        public int GpuUsageGraphColumnSpan
        {
            get => gpuUsageGraphColumnSpan;
            private set => SetProperty(ref gpuUsageGraphColumnSpan, value);
        }

        public int GpuTemperatureGraphColumn
        {
            get => gpuTemperatureGraphColumn;
            private set => SetProperty(ref gpuTemperatureGraphColumn, value);
        }

        public int GpuTemperatureGraphColumnSpan
        {
            get => gpuTemperatureGraphColumnSpan;
            private set => SetProperty(ref gpuTemperatureGraphColumnSpan, value);
        }

        public IReadOnlyDictionary<string, Visibility> SensorVisibilities
        {
            get => sensorVisibilities;
            private set => SetProperty(ref sensorVisibilities, value);
        }

        public IReadOnlyDictionary<string, string> SensorStatistics
        {
            get => sensorStatistics;
            private set => SetProperty(ref sensorStatistics, value);
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

        public MediaGeometry CpuUsageHistoryGeometry
        {
            get => cpuUsageHistoryGeometry;
            private set => SetProperty(ref cpuUsageHistoryGeometry, value);
        }

        public MediaGeometry CpuUsageAreaGeometry
        {
            get => cpuUsageAreaGeometry;
            private set => SetProperty(ref cpuUsageAreaGeometry, value);
        }

        public MediaGeometry CpuTemperatureHistoryGeometry
        {
            get => cpuTemperatureHistoryGeometry;
            private set => SetProperty(ref cpuTemperatureHistoryGeometry, value);
        }

        public MediaGeometry GpuUsageHistoryGeometry
        {
            get => gpuUsageHistoryGeometry;
            private set => SetProperty(ref gpuUsageHistoryGeometry, value);
        }

        public MediaGeometry GpuUsageAreaGeometry
        {
            get => gpuUsageAreaGeometry;
            private set => SetProperty(ref gpuUsageAreaGeometry, value);
        }

        public MediaGeometry GpuTemperatureHistoryGeometry
        {
            get => gpuTemperatureHistoryGeometry;
            private set => SetProperty(ref gpuTemperatureHistoryGeometry, value);
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
            usageWarningPercent =
                Math.Clamp(GetFiniteOrDefault(settings.UsageWarningPercent, 90), 0, 99);
            usageDangerPercent =
                Math.Clamp(GetFiniteOrDefault(settings.UsageDangerPercent, 100), 1, 100);

            if (usageDangerPercent <= usageWarningPercent)
            {
                usageDangerPercent = Math.Min(100, usageWarningPercent + 1);
            }
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
            bool showCpuUsageGraph =
                settings.ShowMiniGraphs &&
                settings.ShowCpuGraph &&
                settings.ShowCpuUsageGraph;
            bool showCpuTemperatureGraph =
                settings.ShowMiniGraphs &&
                settings.ShowCpuGraph &&
                settings.ShowCpuTemperatureGraph;
            bool showGpuUsageGraph =
                settings.ShowMiniGraphs &&
                settings.ShowGpuGraph &&
                settings.ShowGpuUsageGraph;
            bool showGpuTemperatureGraph =
                settings.ShowMiniGraphs &&
                settings.ShowGpuGraph &&
                settings.ShowGpuTemperatureGraph;

            CpuUsageGraphVisibility =
                showCpuUsageGraph ? Visibility.Visible : Visibility.Collapsed;
            CpuTemperatureGraphVisibility =
                showCpuTemperatureGraph ? Visibility.Visible : Visibility.Collapsed;
            GpuUsageGraphVisibility =
                showGpuUsageGraph ? Visibility.Visible : Visibility.Collapsed;
            GpuTemperatureGraphVisibility =
                showGpuTemperatureGraph ? Visibility.Visible : Visibility.Collapsed;
            CpuUsageGraphColumnSpan = showCpuTemperatureGraph ? 1 : 3;
            CpuTemperatureGraphColumn = showCpuUsageGraph ? 2 : 0;
            CpuTemperatureGraphColumnSpan = showCpuUsageGraph ? 1 : 3;
            GpuUsageGraphColumnSpan = showGpuTemperatureGraph ? 1 : 3;
            GpuTemperatureGraphColumn = showGpuUsageGraph ? 2 : 0;
            GpuTemperatureGraphColumnSpan = showGpuUsageGraph ? 1 : 3;
            CpuGraphVisibility =
                showCpuUsageGraph || showCpuTemperatureGraph
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            GpuGraphVisibility =
                showGpuUsageGraph || showGpuTemperatureGraph
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            HashSet<string> hiddenSensors =
                new(settings.HiddenSensors ?? [], StringComparer.Ordinal);
            SensorVisibilities =
                SensorOptionDefinitions.All.ToDictionary(
                    option => option.Id,
                    option => hiddenSensors.Contains(option.Id) ||
                              (option.IsAdvanced && !settings.ShowAdvancedSensors)
                        ? Visibility.Collapsed
                        : Visibility.Visible);

            ApplyDateTimeLanguage(settings.DateTimeLanguage);
            ApplyTheme(settings.Theme);
            UpdateClock(DateTime.Now);
        }

        public void UpdateClock(DateTime now)
        {
            CurrentTime = now.ToString(timeFormat, dateTimeCulture);
            CurrentDate = now.ToString("dddd dd MMMM yyyy", dateTimeCulture);
        }

        public void ResetMonitoringData()
        {
            runningStatistics.Clear();
            cpuUsageHistory.Clear();
            cpuTemperatureHistory.Clear();
            gpuUsageHistory.Clear();
            gpuTemperatureHistory.Clear();

            SensorStatistics =
                SensorOptionDefinitions.All.ToDictionary(
                    option => option.Id,
                    _ => "Min --  Moy --  Max --");

            CpuUsageHistoryPoints = [];
            CpuUsageAreaPoints = [];
            CpuTemperatureHistoryPoints = [];
            GpuUsageHistoryPoints = [];
            GpuUsageAreaPoints = [];
            GpuTemperatureHistoryPoints = [];

            CpuUsageHistoryGeometry = MediaGeometry.Empty;
            CpuUsageAreaGeometry = MediaGeometry.Empty;
            CpuTemperatureHistoryGeometry = MediaGeometry.Empty;
            GpuUsageHistoryGeometry = MediaGeometry.Empty;
            GpuUsageAreaGeometry = MediaGeometry.Empty;
            GpuTemperatureHistoryGeometry = MediaGeometry.Empty;
        }

        private void ApplyDateTimeLanguage(string language)
        {
            if (language == "English")
            {
                dateTimeCulture = new CultureInfo("en-US");
                timeFormat = "hh:mm:ss tt";
                return;
            }

            if (language == "System")
            {
                dateTimeCulture = CultureInfo.CurrentCulture;
                timeFormat = dateTimeCulture.DateTimeFormat.LongTimePattern;
                return;
            }

            dateTimeCulture = new CultureInfo("fr-FR");
            timeFormat = "HH:mm:ss";
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
            (CpuUsageBrush, CpuUsageAreaBrush) =
                GetUsageBrushes(
                    data.CpuUsage,
                    usageWarningPercent,
                    usageDangerPercent);
            (GpuUsageBrush, GpuUsageAreaBrush) =
                GetUsageBrushes(
                    data.GpuUsage,
                    usageWarningPercent,
                    usageDangerPercent);

            UpdateStatistics(data);
            UpdateHistory(data);
        }

        private void UpdateStatistics(SensorData data)
        {
            double vramUsagePercent =
                CalculatePercent(
                    data.GpuMemoryUsedGB,
                    data.GpuMemoryTotalGB,
                    data.GpuMemoryUsagePercent);
            double gpuTensionDisplay =
                data.GpuTension > 0 && data.GpuTension < 1
                    ? data.GpuTension * 1000
                    : data.GpuTension;

            AddStatistic("CpuUsage", data.CpuUsage, "F1", true);
            AddStatistic("CpuTemperature", data.CpuTemperature, "F1");
            AddStatistic("RamUsed", data.RamUsed, "F1", true);
            AddStatistic("CpuClock", data.CpuClock / 1000, "F2");
            AddStatistic("CpuPower", data.CpuPower, "F1");
            AddStatistic("CpuTension", data.CpuTension, "F3");
            AddStatistic("RamUsagePercent", data.RamUsagePercent, "F0", true);
            AddStatistic("RamTotal", data.RamTotal, "F1");
            AddStatistic("RamClock", data.RamClock, "F0");
            AddStatistic("GpuUsage", data.GpuUsage, "F1", true);
            AddStatistic("GpuTemperature", data.GpuTemperature, "F0");
            AddStatistic("GpuMemoryUsed", data.GpuMemoryUsedGB, "F1", true);
            AddStatistic("GpuClock", data.GpuClock, "F0");
            AddStatistic("GpuPower", data.GpuPower, "F1");
            AddStatistic("GpuTension", gpuTensionDisplay, data.GpuTension < 1 ? "F0" : "F3");
            AddStatistic("GpuHotspot", data.GpuHotspot, "F1");
            AddStatistic("TotalPower", data.TotalPower, "F1");
            AddStatistic("GpuMemoryJunction", data.GpuMemoryJunction, "F0");
            AddStatistic("GpuMemoryUsagePercent", vramUsagePercent, "F0", true);
            AddStatistic("GpuMemoryTotal", data.GpuMemoryTotalGB, "F1");
            AddStatistic("Fps", data.Fps, "F0", true);

            SensorStatistics =
                SensorOptionDefinitions.All.ToDictionary(
                    option => option.Id,
                    option => runningStatistics.TryGetValue(option.Id, out RunningStatistics? stats)
                        ? stats.Format()
                        : "Min --  Moy --  Max --");
        }

        private void AddStatistic(
            string id,
            double value,
            string format,
            bool allowZero = false)
        {
            if (!IsFinite(value) || value < 0 || (!allowZero && value <= 0))
            {
                return;
            }

            if (!runningStatistics.TryGetValue(id, out RunningStatistics? statistics))
            {
                statistics = new RunningStatistics(format);
                runningStatistics[id] = statistics;
            }

            statistics.Add(value);
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

            return BrushFromRgb(70, 190, 110);
        }

        private static (MediaBrush Line, MediaBrush Area) GetUsageBrushes(
            double usage,
            double warningThreshold,
            double dangerThreshold)
        {
            if (IsFinite(usage) && usage >= dangerThreshold)
            {
                return (
                    MediaBrushes.Red,
                    BrushFromArgb(64, 255, 0, 0));
            }

            if (IsFinite(usage) && usage >= warningThreshold)
            {
                return (
                    MediaBrushes.Orange,
                    BrushFromArgb(64, 255, 165, 0));
            }

            return (
                MediaBrushes.White,
                BrushFromArgb(48, 255, 255, 255));
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

            CpuUsageHistoryGeometry =
                CreateSmoothGeometry(CpuUsageHistoryPoints);
            CpuUsageAreaGeometry =
                CreateSmoothAreaGeometry(CpuUsageHistoryPoints);
            CpuTemperatureHistoryGeometry =
                CreateSmoothGeometry(CpuTemperatureHistoryPoints);
            GpuUsageHistoryGeometry =
                CreateSmoothGeometry(GpuUsageHistoryPoints);
            GpuUsageAreaGeometry =
                CreateSmoothAreaGeometry(GpuUsageHistoryPoints);
            GpuTemperatureHistoryGeometry =
                CreateSmoothGeometry(GpuTemperatureHistoryPoints);
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

        private static MediaGeometry CreateSmoothGeometry(
            PointCollection points)
        {
            if (points.Count == 0)
            {
                return MediaGeometry.Empty;
            }

            var geometry = new StreamGeometry();

            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(points[0], false, false);
                AddSmoothSegments(context, points);
            }

            geometry.Freeze();
            return geometry;
        }

        private static MediaGeometry CreateSmoothAreaGeometry(
            PointCollection points)
        {
            const double baseline = 164;

            if (points.Count == 0)
            {
                return MediaGeometry.Empty;
            }

            var geometry = new StreamGeometry();

            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(
                    new WpfPoint(points[0].X, baseline),
                    true,
                    true);
                context.LineTo(points[0], true, false);
                AddSmoothSegments(context, points);
                context.LineTo(
                    new WpfPoint(points[^1].X, baseline),
                    true,
                    false);
            }

            geometry.Freeze();
            return geometry;
        }

        private static void AddSmoothSegments(
            StreamGeometryContext context,
            PointCollection points)
        {
            if (points.Count == 1)
            {
                return;
            }

            const double tension = 0.18;

            for (int index = 0; index < points.Count - 1; index++)
            {
                WpfPoint previous = index == 0 ? points[index] : points[index - 1];
                WpfPoint current = points[index];
                WpfPoint next = points[index + 1];
                WpfPoint following =
                    index + 2 < points.Count ? points[index + 2] : next;

                var firstControl = new WpfPoint(
                    current.X + ((next.X - previous.X) * tension),
                    current.Y + ((next.Y - previous.Y) * tension));
                var secondControl = new WpfPoint(
                    next.X - ((following.X - current.X) * tension),
                    next.Y - ((following.Y - current.Y) * tension));

                context.BezierTo(
                    firstControl,
                    secondControl,
                    next,
                    true,
                    false);
            }
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

        private sealed class RunningStatistics(string format)
        {
            private double minimum = double.PositiveInfinity;
            private double maximum = double.NegativeInfinity;
            private double sum;
            private long count;

            public void Add(double value)
            {
                minimum = Math.Min(minimum, value);
                maximum = Math.Max(maximum, value);
                sum += value;
                count++;
            }

            public string Format()
            {
                if (count == 0)
                {
                    return "Min --  Moy --  Max --";
                }

                return string.Create(
                    CultureInfo.InvariantCulture,
                    $"Min {minimum.ToString(format, CultureInfo.InvariantCulture)}  " +
                    $"Moy {(sum / count).ToString(format, CultureInfo.InvariantCulture)}  " +
                    $"Max {maximum.ToString(format, CultureInfo.InvariantCulture)}");
            }
        }
    }
}
