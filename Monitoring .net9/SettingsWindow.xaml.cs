using Microsoft.Win32;
using Monitoring_net9.Models;
using Monitoring_net9.Services;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfComboBoxItem = System.Windows.Controls.ComboBoxItem;
using WpfTextBox = System.Windows.Controls.TextBox;
using Forms = System.Windows.Forms;

namespace Monitoring_net9
{
    public partial class SettingsWindow : Window
    {
        private bool isUpdatingScaleControls;

        public SettingsWindow()
        {
            InitializeComponent();

            AppSettings settings =
                SettingsService.Load();

            BuildSensorOptions(settings);

            foreach (var screen in Forms.Screen.AllScreens)
            {
                ScreenComboBox.Items.Add(screen.DeviceName);
            }

            ScreenComboBox.SelectedItem =
                ScreenComboBox.Items.Contains(settings.SelectedScreen)
                    ? settings.SelectedScreen
                    : ScreenComboBox.Items.Count > 0
                        ? ScreenComboBox.Items[0]
                        : null;

            StartupCheckBox.IsChecked =
                IsStartupEnabled();

            FullscreenCheckBox.IsChecked =
                settings.Fullscreen;

            HwInfoPathTextBox.Text =
                settings.HwInfoPath;

            ThemeComboBox.SelectedItem =
                ThemeComboBox.Items
                    .OfType<WpfComboBoxItem>()
                    .FirstOrDefault(
                        item => item.Content?.ToString() == settings.Theme)
                ?? ThemeComboBox.Items[0];

            SelectDateTimeLanguage(settings.DateTimeLanguage);

            DashboardScaleTextBox.Text =
                settings.DashboardScale.ToString(
                    "F2",
                    CultureInfo.InvariantCulture);
            SelectDashboardScalePreset(
                settings.DashboardScalePreset,
                settings.DashboardScale);

            CpuWarningTextBox.Text =
                settings.CpuWarningTemperature.ToString(
                    "F0",
                    CultureInfo.InvariantCulture);
            CpuDangerTextBox.Text =
                settings.CpuDangerTemperature.ToString(
                    "F0",
                    CultureInfo.InvariantCulture);
            GpuWarningTextBox.Text =
                settings.GpuWarningTemperature.ToString(
                    "F0",
                    CultureInfo.InvariantCulture);
            GpuDangerTextBox.Text =
                settings.GpuDangerTemperature.ToString(
                    "F0",
                    CultureInfo.InvariantCulture);
            UsageWarningTextBox.Text =
                settings.UsageWarningPercent.ToString(
                    "F0",
                    CultureInfo.InvariantCulture);
            UsageDangerTextBox.Text =
                settings.UsageDangerPercent.ToString(
                    "F0",
                    CultureInfo.InvariantCulture);

            ShowAdvancedSensorsCheckBox.IsChecked =
                settings.ShowAdvancedSensors;
            ShowMiniGraphsCheckBox.IsChecked =
                settings.ShowMiniGraphs;
            ApplyGraphOptions(settings);

            SelectHistoryDuration(settings.HistoryDurationSeconds);
        }

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                AppSettings settings =
                    SettingsService.Load();

                settings.Fullscreen =
                    FullscreenCheckBox.IsChecked == true;

                settings.SelectedScreen =
                    ScreenComboBox.SelectedItem?.ToString()
                    ?? "DISPLAY1";

                settings.HwInfoPath =
                    string.IsNullOrWhiteSpace(HwInfoPathTextBox.Text)
                        ? new AppSettings().HwInfoPath
                        : HwInfoPathTextBox.Text.Trim();

                settings.Theme =
                    (ThemeComboBox.SelectedItem as WpfComboBoxItem)
                        ?.Content
                        ?.ToString()
                    ?? "Dark";

                settings.DateTimeLanguage =
                    ReadDateTimeLanguage();

                settings.DashboardScalePreset =
                    ReadDashboardScalePreset();
                settings.DashboardScale =
                    ReadDouble(DashboardScaleTextBox, settings.DashboardScale);
                settings.CpuWarningTemperature =
                    ReadDouble(CpuWarningTextBox, settings.CpuWarningTemperature);
                settings.CpuDangerTemperature =
                    ReadDouble(CpuDangerTextBox, settings.CpuDangerTemperature);
                settings.GpuWarningTemperature =
                    ReadDouble(GpuWarningTextBox, settings.GpuWarningTemperature);
                settings.GpuDangerTemperature =
                    ReadDouble(GpuDangerTextBox, settings.GpuDangerTemperature);
                settings.UsageWarningPercent =
                    ReadDouble(UsageWarningTextBox, settings.UsageWarningPercent);
                settings.UsageDangerPercent =
                    ReadDouble(UsageDangerTextBox, settings.UsageDangerPercent);

                settings.ShowAdvancedSensors =
                    ShowAdvancedSensorsCheckBox.IsChecked == true;
                settings.ShowMiniGraphs =
                    ShowMiniGraphsCheckBox.IsChecked == true;
                settings.ShowCpuUsageGraph =
                    ShowCpuUsageGraphCheckBox.IsChecked == true;
                settings.ShowCpuTemperatureGraph =
                    ShowCpuTemperatureGraphCheckBox.IsChecked == true;
                settings.ShowGpuUsageGraph =
                    ShowGpuUsageGraphCheckBox.IsChecked == true;
                settings.ShowGpuTemperatureGraph =
                    ShowGpuTemperatureGraphCheckBox.IsChecked == true;
                settings.ShowCpuGraph =
                    settings.ShowCpuUsageGraph || settings.ShowCpuTemperatureGraph;
                settings.ShowGpuGraph =
                    settings.ShowGpuUsageGraph || settings.ShowGpuTemperatureGraph;
                settings.HiddenSensors =
                    SensorOptionsPanel.Children
                        .OfType<WpfCheckBox>()
                        .Where(checkBox => checkBox.IsChecked != true)
                        .Select(checkBox => checkBox.Tag?.ToString())
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Cast<string>()
                        .ToList();
                settings.HistoryDurationSeconds =
                    ReadHistoryDuration();

                NormalizeThresholds(settings);

                SettingsService.Save(settings);

                System.Windows.MessageBox.Show(
                    "Param\u00e8tres sauvegard\u00e9s !");

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Settings save UI error: {ex}");
                System.Windows.MessageBox.Show(
                    "Impossible de sauvegarder les param\u00e8tres.");
            }
        }

        private void ResetButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ApplySettingsToControls(new AppSettings());
        }

        private void ResetMonitoringDataButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (Owner is MainWindow mainWindow)
                {
                    mainWindow.ResetMonitoringData();
                    System.Windows.MessageBox.Show(
                        this,
                        "Les valeurs Min, Moy, Max et les graphes ont été remis à zéro.",
                        "CoreView");
                }
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Monitoring data reset error: {ex}");
                System.Windows.MessageBox.Show(
                    this,
                    "Impossible de remettre les mesures à zéro.",
                    "CoreView");
            }
        }

        private void ApplySettingsToControls(AppSettings settings)
        {
            ScreenComboBox.SelectedItem =
                ScreenComboBox.Items.Contains(settings.SelectedScreen)
                    ? settings.SelectedScreen
                    : ScreenComboBox.Items.Count > 0
                        ? ScreenComboBox.Items[0]
                        : null;

            FullscreenCheckBox.IsChecked =
                settings.Fullscreen;

            HwInfoPathTextBox.Text =
                settings.HwInfoPath;

            ThemeComboBox.SelectedItem =
                ThemeComboBox.Items
                    .OfType<WpfComboBoxItem>()
                    .FirstOrDefault(
                        item => item.Content?.ToString() == settings.Theme)
                ?? ThemeComboBox.Items[0];

            SelectDateTimeLanguage(settings.DateTimeLanguage);

            DashboardScaleTextBox.Text =
                settings.DashboardScale.ToString(
                    "F2",
                    CultureInfo.InvariantCulture);
            SelectDashboardScalePreset(
                settings.DashboardScalePreset,
                settings.DashboardScale);

            CpuWarningTextBox.Text =
                settings.CpuWarningTemperature.ToString(
                    "F0",
                    CultureInfo.InvariantCulture);
            CpuDangerTextBox.Text =
                settings.CpuDangerTemperature.ToString(
                    "F0",
                    CultureInfo.InvariantCulture);
            GpuWarningTextBox.Text =
                settings.GpuWarningTemperature.ToString(
                    "F0",
                    CultureInfo.InvariantCulture);
            GpuDangerTextBox.Text =
                settings.GpuDangerTemperature.ToString(
                    "F0",
                    CultureInfo.InvariantCulture);
            UsageWarningTextBox.Text =
                settings.UsageWarningPercent.ToString(
                    "F0",
                    CultureInfo.InvariantCulture);
            UsageDangerTextBox.Text =
                settings.UsageDangerPercent.ToString(
                    "F0",
                    CultureInfo.InvariantCulture);

            ShowAdvancedSensorsCheckBox.IsChecked =
                settings.ShowAdvancedSensors;
            ShowMiniGraphsCheckBox.IsChecked =
                settings.ShowMiniGraphs;
            ApplyGraphOptions(settings);
            ApplySensorOptions(settings);
            SelectHistoryDuration(settings.HistoryDurationSeconds);
        }

        private void BuildSensorOptions(AppSettings settings)
        {
            SensorOptionsPanel.Children.Clear();

            foreach (SensorOption option in SensorOptionDefinitions.All)
            {
                SensorOptionsPanel.Children.Add(
                    new WpfCheckBox
                    {
                        Content = option.DisplayName,
                        Tag = option.Id,
                        Width = 280,
                        FontSize = 20,
                        Foreground = System.Windows.Media.Brushes.White,
                        Margin = new Thickness(0, 0, 12, 12)
                    });
            }

            ApplySensorOptions(settings);
        }

        private void ApplyGraphOptions(AppSettings settings)
        {
            ShowCpuUsageGraphCheckBox.IsChecked =
                settings.ShowCpuGraph && settings.ShowCpuUsageGraph;
            ShowCpuTemperatureGraphCheckBox.IsChecked =
                settings.ShowCpuGraph && settings.ShowCpuTemperatureGraph;
            ShowGpuUsageGraphCheckBox.IsChecked =
                settings.ShowGpuGraph && settings.ShowGpuUsageGraph;
            ShowGpuTemperatureGraphCheckBox.IsChecked =
                settings.ShowGpuGraph && settings.ShowGpuTemperatureGraph;
        }

        private void ApplySensorOptions(AppSettings settings)
        {
            HashSet<string> hiddenSensors =
                new(settings.HiddenSensors ?? [], StringComparer.Ordinal);

            foreach (WpfCheckBox checkBox in
                     SensorOptionsPanel.Children.OfType<WpfCheckBox>())
            {
                checkBox.IsChecked =
                    !hiddenSensors.Contains(checkBox.Tag?.ToString() ?? string.Empty);
            }
        }

        private int ReadHistoryDuration()
        {
            if (HistoryDurationComboBox.SelectedItem is WpfComboBoxItem item &&
                int.TryParse(item.Tag?.ToString(), out int seconds))
            {
                return seconds;
            }

            return new AppSettings().HistoryDurationSeconds;
        }

        private string ReadDateTimeLanguage()
        {
            if (DateTimeLanguageComboBox.SelectedItem is WpfComboBoxItem item)
            {
                return item.Tag?.ToString() ?? "French";
            }

            return new AppSettings().DateTimeLanguage;
        }

        private void SelectDateTimeLanguage(string language)
        {
            DateTimeLanguageComboBox.SelectedItem =
                DateTimeLanguageComboBox.Items
                    .OfType<WpfComboBoxItem>()
                    .FirstOrDefault(item => item.Tag?.ToString() == language)
                ?? DateTimeLanguageComboBox.Items[0];
        }

        private string ReadDashboardScalePreset()
        {
            if (DashboardScalePresetComboBox.SelectedItem is WpfComboBoxItem item)
            {
                return item.Tag?.ToString() ?? "Custom";
            }

            return "Custom";
        }

        private void SelectDashboardScalePreset(
            string preset,
            double scale)
        {
            isUpdatingScaleControls = true;

            try
            {
                string normalizedPreset =
                    IsKnownScalePreset(preset)
                        ? preset
                        : InferScalePreset(scale);

                DashboardScalePresetComboBox.SelectedItem =
                    DashboardScalePresetComboBox.Items
                        .OfType<WpfComboBoxItem>()
                        .FirstOrDefault(
                            item => item.Tag?.ToString() == normalizedPreset)
                    ?? DashboardScalePresetComboBox.Items[3];
            }
            finally
            {
                isUpdatingScaleControls = false;
            }
        }

        private void DashboardScalePresetComboBox_SelectionChanged(
            object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (isUpdatingScaleControls)
            {
                return;
            }

            string preset = ReadDashboardScalePreset();

            if (!TryGetPresetScale(preset, out double scale))
            {
                return;
            }

            isUpdatingScaleControls = true;

            try
            {
                DashboardScaleTextBox.Text =
                    scale.ToString("F2", CultureInfo.InvariantCulture);
            }
            finally
            {
                isUpdatingScaleControls = false;
            }
        }

        private void DashboardScaleTextBox_TextChanged(
            object sender,
            System.Windows.Controls.TextChangedEventArgs e)
        {
            if (isUpdatingScaleControls)
            {
                return;
            }

            double scale =
                ReadDouble(DashboardScaleTextBox, new AppSettings().DashboardScale);

            SelectDashboardScalePreset(
                InferScalePreset(scale),
                scale);
        }

        private static bool TryGetPresetScale(
            string preset,
            out double scale)
        {
            scale = preset switch
            {
                "Small" => 0.85,
                "Medium" => 1.0,
                "Large" => 1.15,
                _ => double.NaN
            };

            return IsFinite(scale);
        }

        private static string InferScalePreset(double scale)
        {
            if (!IsFinite(scale))
            {
                return "Custom";
            }

            if (Math.Abs(scale - 0.85) < 0.01)
            {
                return "Small";
            }

            if (Math.Abs(scale - 1.0) < 0.01)
            {
                return "Medium";
            }

            if (Math.Abs(scale - 1.15) < 0.01)
            {
                return "Large";
            }

            return "Custom";
        }

        private static bool IsKnownScalePreset(string preset)
        {
            return preset is "Small" or "Medium" or "Large" or "Custom";
        }

        private void SelectHistoryDuration(int seconds)
        {
            HistoryDurationComboBox.SelectedItem =
                HistoryDurationComboBox.Items
                    .OfType<WpfComboBoxItem>()
                    .FirstOrDefault(
                        item => item.Tag?.ToString() == seconds.ToString(
                            CultureInfo.InvariantCulture))
                ?? HistoryDurationComboBox.Items[1];
        }

        private static double ReadDouble(
            WpfTextBox textBox,
            double fallback)
        {
            if (double.TryParse(
                    textBox.Text,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double value))
            {
                return IsFinite(value) ? value : fallback;
            }

            if (double.TryParse(
                    textBox.Text,
                    NumberStyles.Float,
                    CultureInfo.CurrentCulture,
                    out value))
            {
                return IsFinite(value) ? value : fallback;
            }

            return fallback;
        }

        private static void NormalizeThresholds(AppSettings settings)
        {
            settings.DashboardScale =
                IsFinite(settings.DashboardScale) ? settings.DashboardScale : 1.0;
            settings.CpuWarningTemperature =
                IsFinite(settings.CpuWarningTemperature)
                    ? settings.CpuWarningTemperature
                    : 70;
            settings.CpuDangerTemperature =
                IsFinite(settings.CpuDangerTemperature)
                    ? settings.CpuDangerTemperature
                    : 90;
            settings.GpuWarningTemperature =
                IsFinite(settings.GpuWarningTemperature)
                    ? settings.GpuWarningTemperature
                    : 80;
            settings.GpuDangerTemperature =
                IsFinite(settings.GpuDangerTemperature)
                    ? settings.GpuDangerTemperature
                    : 95;
            settings.UsageWarningPercent =
                IsFinite(settings.UsageWarningPercent)
                    ? settings.UsageWarningPercent
                    : 90;
            settings.UsageDangerPercent =
                IsFinite(settings.UsageDangerPercent)
                    ? settings.UsageDangerPercent
                    : 100;

            settings.DashboardScale =
                Math.Clamp(settings.DashboardScale, 0.75, 1.35);
            settings.UsageWarningPercent =
                Math.Clamp(settings.UsageWarningPercent, 0, 99);
            settings.UsageDangerPercent =
                Math.Clamp(settings.UsageDangerPercent, 1, 100);

            if (settings.UsageDangerPercent <= settings.UsageWarningPercent)
            {
                settings.UsageDangerPercent =
                    Math.Min(100, settings.UsageWarningPercent + 1);
            }

            if (settings.CpuDangerTemperature <= settings.CpuWarningTemperature)
            {
                settings.CpuDangerTemperature =
                    settings.CpuWarningTemperature + 1;
            }

            if (settings.GpuDangerTemperature <= settings.GpuWarningTemperature)
            {
                settings.GpuDangerTemperature =
                    settings.GpuWarningTemperature + 1;
            }
        }

        private void BrowseHwInfoButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "S\u00e9lectionner HWiNFO",
                    Filter = "Applications (*.exe)|*.exe|Tous les fichiers (*.*)|*.*",
                    CheckFileExists = true,
                    Multiselect = false
                };

                if (File.Exists(HwInfoPathTextBox.Text))
                {
                    dialog.InitialDirectory =
                        Path.GetDirectoryName(HwInfoPathTextBox.Text);
                    dialog.FileName =
                        Path.GetFileName(HwInfoPathTextBox.Text);
                }
                else
                {
                    string defaultDirectory =
                        Path.GetDirectoryName(new AppSettings().HwInfoPath)
                        ?? Environment.GetFolderPath(
                            Environment.SpecialFolder.ProgramFiles);

                    if (Directory.Exists(defaultDirectory))
                    {
                        dialog.InitialDirectory = defaultDirectory;
                    }
                }

                if (dialog.ShowDialog(this) == true)
                {
                    HwInfoPathTextBox.Text = dialog.FileName;
                }
            }
            catch (Exception ex)
            {
                LoggerService.Log($"HWiNFO browse error: {ex.Message}");
            }
        }

        private void StartupCheckBox_Checked(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                using RegistryKey? key =
                    Registry.CurrentUser.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                        true);

                key?.SetValue(
                    "CoreView",
                    Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty);
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Startup registry set error: {ex.Message}");
            }
        }

        private void StartupCheckBox_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                using RegistryKey? key =
                    Registry.CurrentUser.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                        true);

                key?.DeleteValue(
                    "CoreView",
                    false);
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Startup registry delete error: {ex.Message}");
            }
        }

        private static bool IsStartupEnabled()
        {
            try
            {
                using RegistryKey? key =
                    Registry.CurrentUser.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

                return key?.GetValue("CoreView") != null;
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Startup registry read error: {ex.Message}");
                return false;
            }
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
