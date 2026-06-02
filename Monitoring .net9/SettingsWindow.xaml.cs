using Microsoft.Win32;
using Monitoring_net9.Models;
using Monitoring_net9.Services;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using WpfComboBoxItem = System.Windows.Controls.ComboBoxItem;
using WpfTextBox = System.Windows.Controls.TextBox;
using Forms = System.Windows.Forms;

namespace Monitoring_net9
{
    public partial class SettingsWindow : Window
    {
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

            using RegistryKey? key =
                Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

            StartupCheckBox.IsChecked =
                key?.GetValue("CoreView") != null;

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
        }

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
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

            NormalizeThresholds(settings);

            SettingsService.Save(settings);

            System.Windows.MessageBox.Show(
                "Param\u00e8tres sauvegard\u00e9s !");

            DialogResult = true;
            Close();
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
                return value;
            }

            if (double.TryParse(
                    textBox.Text,
                    NumberStyles.Float,
                    CultureInfo.CurrentCulture,
                    out value))
            {
                return value;
            }

            return fallback;
        }

        private static void NormalizeThresholds(AppSettings settings)
        {
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

        private void StartupCheckBox_Checked(
            object sender,
            RoutedEventArgs e)
        {
            using RegistryKey? key =
                Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                    true);

            key?.SetValue(
                "CoreView",
                Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty);
        }

        private void StartupCheckBox_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            using RegistryKey? key =
                Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                    true);

            key?.DeleteValue(
                "CoreView",
                false);
        }
    }
}
