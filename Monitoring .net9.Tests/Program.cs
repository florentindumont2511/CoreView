using Monitoring_net9.Models;
using Monitoring_net9.ViewModels;
using System.Windows;
using System.Windows.Media;

var tests = new List<(string Name, Action Test)>
{
    ("formats missing advanced sensors as placeholders", FormatsMissingAdvancedSensorsAsPlaceholders),
    ("formats gpu voltage unit from volts", FormatsGpuVoltageUnitFromVolts),
    ("applies visibility and theme settings", AppliesVisibilityAndThemeSettings),
    ("records bounded history points", RecordsBoundedHistoryPoints),
    ("uses configured history duration", UsesConfiguredHistoryDuration),
    ("updates hwinfo status text", UpdatesHwInfoStatusText),
    ("ignores invalid sensor values", IgnoresInvalidSensorValues),
    ("updates hardware names", UpdatesHardwareNames),
    ("formats extended metrics", FormatsExtendedMetrics),
    ("calculates vram usage percent from used and total", CalculatesVramUsagePercentFromUsedAndTotal),
    ("formats date and time using selected language", FormatsDateAndTimeUsingSelectedLanguage),
    ("applies sensor and graph visibility choices", AppliesSensorAndGraphVisibilityChoices),
    ("applies usage and temperature colors", AppliesUsageAndTemperatureColors),
    ("tracks minimum average and maximum values", TracksMinimumAverageAndMaximumValues),
    ("resets statistics and graph history", ResetsStatisticsAndGraphHistory)
};

foreach ((string name, Action test) in tests)
{
    test();
    Console.WriteLine($"PASS {name}");
}

static void FormatsMissingAdvancedSensorsAsPlaceholders()
{
    var viewModel = new MainWindowViewModel();

    viewModel.UpdateSensors(
        new SensorData
        {
            CpuUsage = 12.34,
            RamUsed = 8.5,
            GpuUsage = 56.78,
            GpuMemoryUsedGB = 6.2
        });

    AssertEqual("12.3", viewModel.CpuUsage);
    AssertEqual("--", viewModel.CpuTemperature);
    AssertEqual("--", viewModel.CpuClock);
    AssertEqual("8.5", viewModel.RamUsage);
    AssertEqual("56.8", viewModel.GpuUsage);
    AssertEqual("--", viewModel.GpuTension);
    AssertEqual(string.Empty, viewModel.GpuTensionUnit);
}

static void FormatsGpuVoltageUnitFromVolts()
{
    var viewModel = new MainWindowViewModel();

    viewModel.UpdateSensors(
        new SensorData
        {
            GpuTension = 1.025
        });

    AssertEqual("1.025", viewModel.GpuTension);
    AssertEqual("V", viewModel.GpuTensionUnit);

    viewModel.UpdateSensors(
        new SensorData
        {
            GpuTension = 0.975
        });

    AssertEqual("975", viewModel.GpuTension);
    AssertEqual("mV", viewModel.GpuTensionUnit);
}

static void AppliesVisibilityAndThemeSettings()
{
    var viewModel = new MainWindowViewModel();

    viewModel.ApplySettings(
        new AppSettings
        {
            DashboardScale = 2,
            Theme = "Contrast",
            ShowAdvancedSensors = false,
            ShowMiniGraphs = false
        });

    AssertEqual(1.35, viewModel.DashboardScale);
    AssertEqual(Visibility.Collapsed, viewModel.AdvancedSensorsVisibility);
    AssertEqual(Visibility.Collapsed, viewModel.MiniGraphsVisibility);
    AssertEqual(Brushes.Black, viewModel.DashboardBrush);
}

static void RecordsBoundedHistoryPoints()
{
    var viewModel = new MainWindowViewModel();

    for (int i = 0; i < 75; i++)
    {
        viewModel.UpdateSensors(
            new SensorData
            {
                CpuUsage = i,
                CpuTemperature = i,
                GpuUsage = i,
                GpuTemperature = i
            });
    }

    AssertEqual(60, viewModel.CpuUsageHistoryPoints.Count);
    AssertEqual(60, viewModel.GpuTemperatureHistoryPoints.Count);
    AssertTrue(viewModel.CpuUsageAreaPoints.Count > viewModel.CpuUsageHistoryPoints.Count);
}

static void UsesConfiguredHistoryDuration()
{
    var viewModel = new MainWindowViewModel();

    viewModel.ApplySettings(
        new AppSettings
        {
            HistoryDurationSeconds = 30
        });

    for (int i = 0; i < 75; i++)
    {
        viewModel.UpdateSensors(
            new SensorData
            {
                CpuUsage = i,
                GpuUsage = i
            });
    }

    AssertEqual("30s", viewModel.HistoryDurationLabel);
    AssertEqual(30, viewModel.CpuUsageHistoryPoints.Count);
}

static void UpdatesHwInfoStatusText()
{
    var viewModel = new MainWindowViewModel();

    viewModel.UpdateHwInfoStatus(true);
    AssertEqual("HWiNFO connecté", viewModel.HwInfoStatus);

    viewModel.UpdateHwInfoStatus(false);
    AssertEqual("HWiNFO déconnecté", viewModel.HwInfoStatus);
}

static void IgnoresInvalidSensorValues()
{
    var viewModel = new MainWindowViewModel();

    viewModel.UpdateSensors(
        new SensorData
        {
            CpuUsage = double.NaN,
            CpuTemperature = double.PositiveInfinity,
            GpuUsage = double.NegativeInfinity,
            GpuTemperature = double.NaN
        });

    AssertEqual("--", viewModel.CpuUsage);
    AssertEqual("--", viewModel.CpuTemperature);
    AssertEqual("--", viewModel.GpuUsage);
    AssertEqual("--", viewModel.GpuTemperature);
    AssertEqual(1, viewModel.CpuUsageHistoryPoints.Count);
}

static void UpdatesHardwareNames()
{
    var viewModel = new MainWindowViewModel();

    viewModel.UpdateSensors(
        new SensorData
        {
            CpuName = "AMD Ryzen Test",
            GpuName = "AMD Radeon Test"
        });

    AssertEqual("AMD Ryzen Test", viewModel.CpuName);
    AssertEqual("AMD Radeon Test", viewModel.GpuName);
}

static void FormatsExtendedMetrics()
{
    var viewModel = new MainWindowViewModel();

    viewModel.UpdateSensors(
        new SensorData
        {
            RamTotal = 32,
            RamUsagePercent = 67,
            RamClock = 3200,
            GpuMemoryTotalGB = 16,
            GpuMemoryUsagePercent = 42,
            Fps = 144,
            TotalPower = 321.5
        });

    AssertEqual("32.0", viewModel.RamTotal);
    AssertEqual("67", viewModel.RamUsagePercent);
    AssertEqual("3200", viewModel.RamClock);
    AssertEqual("16.0", viewModel.GpuMemoryTotal);
    AssertEqual("42", viewModel.GpuMemoryUsagePercent);
    AssertEqual("144", viewModel.Fps);
    AssertEqual("321.5", viewModel.TotalPower);
}

static void CalculatesVramUsagePercentFromUsedAndTotal()
{
    var viewModel = new MainWindowViewModel();

    viewModel.UpdateSensors(
        new SensorData
        {
            GpuMemoryUsedGB = 4.7,
            GpuMemoryTotalGB = 16,
            GpuMemoryUsagePercent = 1
        });

    AssertEqual("29", viewModel.GpuMemoryUsagePercent);
}

static void FormatsDateAndTimeUsingSelectedLanguage()
{
    var viewModel = new MainWindowViewModel();

    viewModel.ApplySettings(
        new AppSettings
        {
            DateTimeLanguage = "English"
        });

    viewModel.UpdateClock(new DateTime(2026, 6, 7, 15, 4, 47));

    AssertEqual("03:04:47 PM", viewModel.CurrentTime);
    AssertEqual("Sunday 07 June 2026", viewModel.CurrentDate);
}

static void AppliesSensorAndGraphVisibilityChoices()
{
    var viewModel = new MainWindowViewModel();

    viewModel.ApplySettings(
        new AppSettings
        {
            ShowMiniGraphs = true,
            ShowCpuGraph = true,
            ShowGpuGraph = true,
            ShowCpuUsageGraph = false,
            ShowCpuTemperatureGraph = true,
            ShowGpuUsageGraph = true,
            ShowGpuTemperatureGraph = false,
            HiddenSensors = ["CpuUsage", "GpuMemoryTotal"]
        });

    AssertEqual(Visibility.Collapsed, viewModel.SensorVisibilities["CpuUsage"]);
    AssertEqual(Visibility.Visible, viewModel.SensorVisibilities["GpuUsage"]);
    AssertEqual(Visibility.Collapsed, viewModel.SensorVisibilities["GpuMemoryTotal"]);
    AssertEqual(Visibility.Visible, viewModel.CpuGraphVisibility);
    AssertEqual(Visibility.Visible, viewModel.GpuGraphVisibility);
    AssertEqual(Visibility.Collapsed, viewModel.CpuUsageGraphVisibility);
    AssertEqual(Visibility.Visible, viewModel.CpuTemperatureGraphVisibility);
    AssertEqual(0, viewModel.CpuTemperatureGraphColumn);
    AssertEqual(3, viewModel.CpuTemperatureGraphColumnSpan);
    AssertEqual(Visibility.Visible, viewModel.GpuUsageGraphVisibility);
    AssertEqual(Visibility.Collapsed, viewModel.GpuTemperatureGraphVisibility);
    AssertEqual(3, viewModel.GpuUsageGraphColumnSpan);
}

static void AppliesUsageAndTemperatureColors()
{
    var viewModel = new MainWindowViewModel();

    viewModel.UpdateSensors(
        new SensorData
        {
            CpuUsage = 50,
            GpuUsage = 90,
            CpuTemperature = 50,
            GpuTemperature = 96
        });

    AssertEqual(Colors.White, ((SolidColorBrush)viewModel.CpuUsageBrush).Color);
    AssertEqual(Colors.Orange, ((SolidColorBrush)viewModel.GpuUsageBrush).Color);
    AssertEqual(Color.FromRgb(70, 190, 110), ((SolidColorBrush)viewModel.CpuTemperatureBrush).Color);
    AssertEqual(Colors.Red, ((SolidColorBrush)viewModel.GpuTemperatureBrush).Color);

    viewModel.UpdateSensors(new SensorData { CpuUsage = 100 });
    AssertEqual(Colors.Red, ((SolidColorBrush)viewModel.CpuUsageBrush).Color);

    viewModel.ApplySettings(
        new AppSettings
        {
            UsageWarningPercent = 80,
            UsageDangerPercent = 95
        });
    viewModel.UpdateSensors(
        new SensorData
        {
            CpuUsage = 80,
            GpuUsage = 95
        });

    AssertEqual(Colors.Orange, ((SolidColorBrush)viewModel.CpuUsageBrush).Color);
    AssertEqual(Colors.Red, ((SolidColorBrush)viewModel.GpuUsageBrush).Color);
}

static void TracksMinimumAverageAndMaximumValues()
{
    var viewModel = new MainWindowViewModel();

    viewModel.UpdateSensors(
        new SensorData
        {
            CpuUsage = 20,
            CpuTemperature = 40,
            GpuMemoryUsedGB = 4,
            GpuMemoryTotalGB = 16
        });
    viewModel.UpdateSensors(
        new SensorData
        {
            CpuUsage = 60,
            CpuTemperature = 60,
            GpuMemoryUsedGB = 8,
            GpuMemoryTotalGB = 16
        });

    AssertEqual("Min 20.0  Moy 40.0  Max 60.0", viewModel.SensorStatistics["CpuUsage"]);
    AssertEqual("Min 40.0  Moy 50.0  Max 60.0", viewModel.SensorStatistics["CpuTemperature"]);
    AssertEqual("Min 25  Moy 38  Max 50", viewModel.SensorStatistics["GpuMemoryUsagePercent"]);
}

static void ResetsStatisticsAndGraphHistory()
{
    var viewModel = new MainWindowViewModel();

    viewModel.UpdateSensors(
        new SensorData
        {
            CpuUsage = 42,
            CpuTemperature = 55,
            GpuUsage = 60,
            GpuTemperature = 70
        });

    viewModel.ResetMonitoringData();

    AssertEqual("Min --  Moy --  Max --", viewModel.SensorStatistics["CpuUsage"]);
    AssertEqual(0, viewModel.CpuUsageHistoryPoints.Count);
    AssertEqual(0, viewModel.GpuTemperatureHistoryPoints.Count);
    AssertTrue(viewModel.CpuUsageHistoryGeometry.IsEmpty());
}

static void AssertEqual<T>(
    T expected,
    T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException(
            $"Expected '{expected}', got '{actual}'.");
    }
}

static void AssertTrue(bool condition)
{
    if (!condition)
    {
        throw new InvalidOperationException("Expected condition to be true.");
    }
}
