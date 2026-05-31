using Hardcodet.Wpf.TaskbarNotification;
using Monitoring_net9.Models;
using Monitoring_net9.Services;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace Monitoring_net9
{
    public partial class MainWindow : Window
    {
        private readonly MonitoringManager monitoringManager;
        private readonly DispatcherTimer timer;
        private readonly DispatcherTimer hwInfoRestartTimer;
        private readonly TaskbarIcon trayIcon;

        private SettingsWindow? settingsWindow;
        private AppSettings settings;

        public MainWindow()
        {
            InitializeComponent();

            settings = SettingsService.Load();
            Topmost = true;
            ShowInTaskbar = false;
            RenderOptions.ProcessRenderMode =
                System.Windows.Interop.RenderMode.SoftwareOnly;

            trayIcon = CreateTrayIcon();

            Loaded += (_, _) => MoveToMonitoringScreen();
            ContentRendered += (_, _) => MoveToMonitoringScreen();

            monitoringManager = new MonitoringManager();
            monitoringManager.Initialize();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();

            hwInfoRestartTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromHours(11)
            };
            hwInfoRestartTimer.Tick += (_, _) => monitoringManager.RestartHwInfo();
            hwInfoRestartTimer.Start();
        }

        private TaskbarIcon CreateTrayIcon()
        {
            var icon = new TaskbarIcon
            {
                Icon = new System.Drawing.Icon("Assets/monitoring.ico"),
                ToolTipText = "Monitoring Dashboard",
                ContextMenu = new ContextMenu()
            };

            var settingsItem = new MenuItem
            {
                Header = "Options"
            };
            settingsItem.Click += (_, _) => OpenSettingsWindow();
            icon.ContextMenu.Items.Add(settingsItem);

            var quitItem = new MenuItem
            {
                Header = "Quitter"
            };
            quitItem.Click += (_, _) => System.Windows.Application.Current.Shutdown();
            icon.ContextMenu.Items.Add(quitItem);

            icon.TrayMouseDoubleClick += TrayIcon_TrayMouseDoubleClick;

            return icon;
        }

        private void MoveToMonitoringScreen()
        {
            var targetScreen =
                Forms.Screen.AllScreens.FirstOrDefault(
                    s => s.DeviceName.Contains(settings.SelectedScreen))
                ?? Forms.Screen.AllScreens.LastOrDefault();

            if (targetScreen == null)
            {
                return;
            }

            WindowState = WindowState.Normal;

            var source = PresentationSource.FromVisual(this);
            double dpiX = source?.CompositionTarget?.TransformFromDevice.M11 ?? 1.0;
            double dpiY = source?.CompositionTarget?.TransformFromDevice.M22 ?? 1.0;

            Left = targetScreen.Bounds.Left * dpiX;
            Top = targetScreen.Bounds.Top * dpiY;

            if (settings.Fullscreen)
            {
                Width = targetScreen.Bounds.Width * dpiX;
                Height = targetScreen.Bounds.Height * dpiY;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;
                WindowState = WindowState.Normal;
                Width = 1280;
                Height = 720;
            }

            Activate();
        }

        private void TrayIcon_TrayMouseDoubleClick(
            object sender,
            RoutedEventArgs e)
        {
            if (IsVisible)
            {
                Hide();
                return;
            }

            Show();
            MoveToMonitoringScreen();
            Activate();
        }

        private void SettingsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenSettingsWindow();
        }

        private void OpenSettingsWindow()
        {
            if (settingsWindow == null || !settingsWindow.IsLoaded)
            {
                settingsWindow = new SettingsWindow();
            }

            settingsWindow.Owner = this;
            settingsWindow.Topmost = true;
            settingsWindow.ShowDialog();
            settings = SettingsService.Load();
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                System.Windows.Application.Current.Shutdown();
            }

            if (e.SystemKey == Key.F4)
            {
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            timer.Stop();
            hwInfoRestartTimer.Stop();
            trayIcon.Dispose();
            monitoringManager.Dispose();

            base.OnClosed(e);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            TimeText.Text = DateTime.Now.ToString("HH:mm:ss");
            DateText.Text = DateTime.Now.ToString(
                "dddd dd MMMM yyyy",
                new CultureInfo("fr-FR"));

            try
            {
                monitoringManager.Update();
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Update Screen Error: {ex.Message}");
            }

            UpdateCpuUi();
            UpdateGpuUi();
        }

        private void UpdateCpuUi()
        {
            CpuUsageText.Text = $"{monitoringManager.Data.CpuUsage:F1}";
            CpuTempText.Text = $"{monitoringManager.Data.CpuTemperature:F1}";
            CpuTempText.Foreground =
                GetTemperatureBrush(monitoringManager.Data.CpuTemperature, 70, 90);
            CpuClockText.Text = $"{monitoringManager.Data.CpuClock / 1000:F2}";
            CpuPowerText.Text = $"{monitoringManager.Data.CpuPower:F1}";
            CpuTensionText.Text = $"{monitoringManager.Data.CpuTension:F3}";
            RamUsageText.Text = $"{monitoringManager.Data.RamUsed:F1}";
        }

        private void UpdateGpuUi()
        {
            GpuUsageText.Text = $"{monitoringManager.Data.GpuUsage:F1}";
            GpuTempText.Text = $"{monitoringManager.Data.GpuTemperature:F1}";
            GpuTempText.Foreground =
                GetTemperatureBrush(monitoringManager.Data.GpuTemperature, 80, 95);
            GpuMemoryText.Text = $"{monitoringManager.Data.GpuMemoryUsedGB:F1}";
            GpuClockText.Text = $"{monitoringManager.Data.GpuClock:F0}";
            GpuHotspotText.Text = $"{monitoringManager.Data.GpuHotspot:F1}";
            GpuMemoryJunctionText.Text =
                $"{monitoringManager.Data.GpuMemoryJunction:F1}";
            GpuPowerText.Text = $"{monitoringManager.Data.GpuPower:F1}";

            if (monitoringManager.Data.GpuTension >= 1)
            {
                GpuTensionText.Text = $"{monitoringManager.Data.GpuTension:F3}";
                GpuTensionUnitText.Text = "V";
                return;
            }

            GpuTensionText.Text = $"{monitoringManager.Data.GpuTension * 1000:F0}";
            GpuTensionUnitText.Text = "mV";
        }

        private static System.Windows.Media.Brush GetTemperatureBrush(
            double temperature,
            double warningThreshold,
            double dangerThreshold)
        {
            if (temperature > dangerThreshold)
            {
                return System.Windows.Media.Brushes.Red;
            }

            if (temperature > warningThreshold)
            {
                return System.Windows.Media.Brushes.Orange;
            }

            return System.Windows.Media.Brushes.White;
        }
    }
}
