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
    ("updates hwinfo status text", UpdatesHwInfoStatusText)
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
