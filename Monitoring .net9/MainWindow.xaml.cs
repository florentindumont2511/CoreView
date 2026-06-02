using Hardcodet.Wpf.TaskbarNotification;
using Monitoring_net9.Models;
using Monitoring_net9.Services;
using Monitoring_net9.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private readonly MainWindowViewModel viewModel = new();

        private SettingsWindow? settingsWindow;
        private AppSettings settings;
        private bool isRestartingHwInfo;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = viewModel;
            settings = SettingsService.Load();
            viewModel.ApplySettings(settings);
            Topmost = true;
            ShowInTaskbar = false;
            System.Windows.Media.RenderOptions.ProcessRenderMode =
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
            hwInfoRestartTimer.Tick += HwInfoRestartTimer_Tick;
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
            bool? result = settingsWindow.ShowDialog();

            if (result == true)
            {
                settings = SettingsService.Load();
                viewModel.ApplySettings(settings);
                MoveToMonitoringScreen();
            }
        }

        private async void HwInfoRestartTimer_Tick(
            object? sender,
            EventArgs e)
        {
            if (isRestartingHwInfo)
            {
                return;
            }

            isRestartingHwInfo = true;

            try
            {
                await monitoringManager.RestartHwInfoAsync();
            }
            catch (Exception ex)
            {
                LoggerService.Log($"HWiNFO restart error: {ex.Message}");
            }
            finally
            {
                isRestartingHwInfo = false;
            }
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
            viewModel.UpdateClock(DateTime.Now);

            try
            {
                monitoringManager.Update();
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Update Screen Error: {ex.Message}");
            }

            viewModel.UpdateSensors(monitoringManager.Data);
        }
    }
}
