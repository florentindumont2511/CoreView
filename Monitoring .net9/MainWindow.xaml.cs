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
        private readonly DispatcherTimer renderTimer;
        private readonly DispatcherTimer hwInfoRestartTimer;
        private readonly TaskbarIcon trayIcon;
        private readonly MainWindowViewModel viewModel = new();
        private readonly CancellationTokenSource monitoringCancellation = new();
        private readonly SemaphoreSlim monitoringGate = new(1, 1);
        private readonly object latestDataSync = new();

        private SettingsWindow? settingsWindow;
        private AppSettings settings;
        private bool isRestartingHwInfo;
        private Task? monitoringTask;
        private SensorData? latestSensorData;
        private bool latestHwInfoStatus;
        private long latestSensorVersion;
        private long renderedSensorVersion;

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

            renderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            renderTimer.Tick += RenderTimer_Tick;
            renderTimer.Start();

            monitoringTask =
                Task.Run(() => MonitoringLoopAsync(monitoringCancellation.Token));

            hwInfoRestartTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromHours(11)
            };
            hwInfoRestartTimer.Tick += HwInfoRestartTimer_Tick;
            hwInfoRestartTimer.Start();
        }

        private TaskbarIcon CreateTrayIcon()
        {
            string iconPath =
                System.IO.Path.Combine(
                    AppContext.BaseDirectory,
                    "Assets",
                    "monitoring.ico");

            var icon = new TaskbarIcon
            {
                ToolTipText = "Monitoring Dashboard",
                ContextMenu = new ContextMenu()
            };

            if (System.IO.File.Exists(iconPath))
            {
                icon.Icon = new System.Drawing.Icon(iconPath);
            }

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
            try
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
            catch (Exception ex)
            {
                LoggerService.Log($"Move screen error: {ex.Message}");
            }
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

        internal void ResetMonitoringData()
        {
            viewModel.ResetMonitoringData();
        }

        private void ResetMonitoringDataButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ResetMonitoringData();
        }

        private void OpenSettingsWindow()
        {
            try
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
            catch (Exception ex)
            {
                LoggerService.Log($"Settings window error: {ex}");
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
                await monitoringGate.WaitAsync();

                try
                {
                    await monitoringManager.RestartHwInfoAsync();
                }
                finally
                {
                    monitoringGate.Release();
                }
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
            try
            {
                renderTimer.Stop();
                hwInfoRestartTimer.Stop();
                monitoringCancellation.Cancel();
                monitoringTask?.Wait(TimeSpan.FromSeconds(3));
                trayIcon.Dispose();
                monitoringManager.Dispose();
                monitoringGate.Dispose();
                monitoringCancellation.Dispose();
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Shutdown cleanup error: {ex.Message}");
            }

            base.OnClosed(e);
        }

        private async Task MonitoringLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await monitoringGate.WaitAsync(cancellationToken);

                    try
                    {
                        monitoringManager.Update();
                        SensorData snapshot = monitoringManager.Data.Copy();
                        bool isHwInfoConnected = monitoringManager.IsHwInfoConnected;

                        lock (latestDataSync)
                        {
                            latestSensorData = snapshot;
                            latestHwInfoStatus = isHwInfoConnected;
                            latestSensorVersion++;
                        }
                    }
                    finally
                    {
                        monitoringGate.Release();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LoggerService.Log($"Sensor collection error: {ex.Message}");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                viewModel.UpdateClock(DateTime.Now);

                SensorData? snapshot;
                bool isHwInfoConnected;
                long version;

                lock (latestDataSync)
                {
                    version = latestSensorVersion;

                    if (version == renderedSensorVersion)
                    {
                        return;
                    }

                    snapshot = latestSensorData;
                    isHwInfoConnected = latestHwInfoStatus;
                }

                if (snapshot == null)
                {
                    return;
                }

                viewModel.UpdateHwInfoStatus(isHwInfoConnected);
                viewModel.UpdateSensors(snapshot);
                renderedSensorVersion = version;
            }
            catch (Exception ex)
            {
                LoggerService.Log($"Update Screen Error: {ex.Message}");
            }
        }
    }
}
