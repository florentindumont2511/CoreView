using Monitoring_net9.Services;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;



namespace Monitoring_net9
{
    public partial class MainWindow : Window
    {
        private readonly MonitoringManager monitoringManager;

        private readonly DispatcherTimer timer;

        private readonly DispatcherTimer hwInfoRestartTimer;

        private void MoveToMonitoringScreen()
        {
            var screens = System.Windows.Forms.Screen.AllScreens;

            var targetScreen =
                screens.FirstOrDefault(
                    s => s.DeviceName.Contains("DISPLAY3"));

            if (targetScreen == null)
                return;

            WindowState = WindowState.Normal;

            var source = PresentationSource.FromVisual(this);

            double dpiX = 1.0;
            double dpiY = 1.0;

            if (source?.CompositionTarget != null)
            {
                dpiX = source.CompositionTarget.TransformFromDevice.M11;
                dpiY = source.CompositionTarget.TransformFromDevice.M22;
            }

            Left = targetScreen.Bounds.Left * dpiX;
            Top = targetScreen.Bounds.Top * dpiY;

            Width = targetScreen.Bounds.Width * dpiX;
            Height = targetScreen.Bounds.Height * dpiY;

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;

            Activate();
        }


        private void MainWindow_ContentRendered(
            object? sender,
            EventArgs e)
                {
                    MoveToMonitoringScreen();
                }

        public MainWindow()
        {
            Loaded += (_, _) => MoveToMonitoringScreen();
            Topmost = true;
            InitializeComponent();

            RenderOptions.ProcessRenderMode =
    System.Windows.Interop.RenderMode.SoftwareOnly;


            ContentRendered += MainWindow_ContentRendered;


            monitoringManager = new MonitoringManager();

            monitoringManager.Initialize();

            monitoringManager.StartHwInfo();

            Thread.Sleep(5000);

            /*
            //Sert à afficher les sondes de HwInfo 64 pour les noms exactes
            string result = "";


            foreach (var reading in monitoringManager.Readings)
            {
                if (reading.LabelOrig.Contains(
                    "GPU",
                    StringComparison.OrdinalIgnoreCase))
                {
                    result +=
                        $"{reading.LabelOrig} = {reading.Value} {reading.Unit}\n";
                }
            }

            System.Windows.MessageBox.Show(result);*/


            //  TIMER POUR RAFRAICHISSEMENT ECRAN
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();


            // TIMER RESTART HWiNFO
            hwInfoRestartTimer = new DispatcherTimer();


            hwInfoRestartTimer.Interval = TimeSpan.FromHours(11);

            hwInfoRestartTimer.Tick += HwInfoRestartTimer_Tick;

            hwInfoRestartTimer.Start();
        }

        private void HwInfoRestartTimer_Tick(
            object? sender,
            EventArgs e)
        {
            monitoringManager.RestartHwInfo();
        }

        protected override void OnKeyDown(
    System.Windows.Input.KeyEventArgs e)
        {
            // Echap pour quitter
            if (e.Key == Key.Escape)
            {
                System.Windows.Application.Current.Shutdown();
            }

            // Bloque ALT + F4
            if (e.SystemKey == Key.F4)
            {
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            TimeText.Text = DateTime.Now.ToString("HH:mm:ss");

            DateText.Text = DateTime.Now.ToString(
                "dddd dd MMMM yyyy",
                new CultureInfo("fr-FR")); // Force la date en format français : JJ/MM/AAAA

            try
            {
                monitoringManager.Update();
            }
            catch (Exception ex)
            {
                LoggerService.Log(
                    $"Update Screen Error: {ex.Message}");
            }


            CpuUsageText.Text =
                $"{monitoringManager.Data.CpuUsage:F1} %";


            CpuTempText.Text =
                $"{monitoringManager.Data.CpuTemperature:F1} °C";
            
            if (monitoringManager.Data.CpuTemperature > 90)
            {
                CpuTempText.Foreground = System.Windows.Media.Brushes.Red;
            }
            else if (monitoringManager.Data.CpuTemperature > 70)
            {
                CpuTempText.Foreground = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                CpuTempText.Foreground = System.Windows.Media.Brushes.White;
            }

            CpuClockText.Text =
                $"{monitoringManager.Data.CpuClock / 1000:F2} GHz";

            CpuPowerText.Text =
                $"{monitoringManager.Data.CpuPower:F1} W";

            RamUsageText.Text =
                $"{monitoringManager.Data.RamUsed:F1} GB";

            GpuUsageText.Text =
                $"{monitoringManager.Data.GpuUsage:F1} %";

            GpuTempText.Text =
                $"{monitoringManager.Data.GpuTemperature:F1} °C";


            if (monitoringManager.Data.GpuTemperature > 95)
            {
                GpuTempText.Foreground = System.Windows.Media.Brushes.Red;
            }
            else if (monitoringManager.Data.GpuTemperature > 80)
            {
                GpuTempText.Foreground = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                GpuTempText.Foreground = System.Windows.Media.Brushes.White;
            }

            GpuMemoryText.Text =
                $"{monitoringManager.Data.GpuMemoryUsedGB:F1} GB";

            GpuClockText.Text =
                $"{monitoringManager.Data.GpuClock:F0} MHz";

            GpuHotspotText.Text =
                $"{monitoringManager.Data.GpuHotspot:F1} °C";

            GpuMemoryJunctionText.Text =
                $"{monitoringManager.Data.GpuMemoryJunction:F1} °C";

            GpuPowerText.Text =
                $"{monitoringManager.Data.GpuPower:F1} W";
        }
    }
}