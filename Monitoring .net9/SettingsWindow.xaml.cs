using Microsoft.Win32;
using Monitoring_net9.Models;
using Monitoring_net9.Services;
using System.Diagnostics;
using System.Windows;
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

            SettingsService.Save(settings);

            System.Windows.MessageBox.Show(
                "Paramètres sauvegardés !\nRelancez le logiciel pour que les paramètres prennent effet.");
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
