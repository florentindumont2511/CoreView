using Monitoring_net9.Models;
using Monitoring_net9.Services;
using System.Windows;
using Forms = System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;

namespace Monitoring_.net9
{
    /// <summary>
    /// Logique d'interaction pour SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            foreach (var screen in Forms.Screen.AllScreens)
            {
                ScreenComboBox.Items.Add(
                    screen.DeviceName);
            }

            RegistryKey key =
            Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

            StartupCheckBox.IsChecked =
                key.GetValue("CoreView") != null;
        }

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
                {
                    AppSettings settings =
                        SettingsService.Load();

                    settings.SelectedScreen =
                        ScreenComboBox.SelectedItem?.ToString()
                        ?? "DISPLAY1";

                    SettingsService.Save(settings);

                    System.Windows.MessageBox.Show(
                        "Écran sauvegardé !");
                }

        private void StartupCheckBox_Checked(
            object sender,
            RoutedEventArgs e)
        {
            RegistryKey? key =
                Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                    true);

            if (key != null)
            {
                key.SetValue(
                    "CoreView",
                    Process.GetCurrentProcess().MainModule?.FileName);
            }
        }

        private void StartupCheckBox_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            RegistryKey? key =
                Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                    true);

            key?.DeleteValue(
                "CoreView",
                false);
        }
    }
}
