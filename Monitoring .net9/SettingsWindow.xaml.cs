using Microsoft.Win32;
using Monitoring_net9.Models;
using Monitoring_net9.Services;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
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

            ShowAdvancedSensorsCheckBox.IsChecked =
                settings.ShowAdvancedSensors;
            ShowMiniGraphsCheckBox.IsChecked =
                settings.ShowMiniGraphs;

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

                settings.ShowAdvancedSensors =
                    ShowAdvancedSensorsCheckBox.IsChecked == true;
                settings.ShowMiniGraphs =
                    ShowMiniGraphsCheckBox.IsChecked == true;
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

            ShowAdvancedSensorsCheckBox.IsChecked =
                settings.ShowAdvancedSensors;
            ShowMiniGraphsCheckBox.IsChecked =
                settings.ShowMiniGraphs;
            SelectHistoryDuration(settings.HistoryDurationSeconds);
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

            settings.DashboardScale =
                Math.Clamp(settings.DashboardScale, 0.75, 1.35);

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
