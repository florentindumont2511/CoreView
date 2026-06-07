using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Monitoring_net9
{
    public partial class App : System.Windows.Application
    {
        private const string SingleInstanceMutexName =
            @"Local\CoreView_Monitoring_Dashboard";

        private Mutex? singleInstanceMutex;
        private bool ownsSingleInstanceMutex;

        protected override void OnStartup(
            System.Windows.StartupEventArgs e)
        {
            singleInstanceMutex =
                new Mutex(
                    true,
                    SingleInstanceMutexName,
                    out ownsSingleInstanceMutex);

            if (!ownsSingleInstanceMutex)
            {
                Shutdown();
                return;
            }

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException +=
                CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException +=
                TaskScheduler_UnobservedTaskException;

            base.OnStartup(e);

            try
            {
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Services.LoggerService.Log($"Startup error: {ex}");
                Shutdown();
            }
        }

        private static void App_DispatcherUnhandledException(
            object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            Services.LoggerService.Log(
                $"UI unhandled exception: {e.Exception}");

            e.Handled = true;
        }

        private static void CurrentDomain_UnhandledException(
            object sender,
            UnhandledExceptionEventArgs e)
        {
            Services.LoggerService.Log(
                $"AppDomain unhandled exception: {e.ExceptionObject}");
        }

        private static void TaskScheduler_UnobservedTaskException(
            object? sender,
            UnobservedTaskExceptionEventArgs e)
        {
            Services.LoggerService.Log(
                $"Task unobserved exception: {e.Exception}");

            e.SetObserved();
        }

        protected override void OnExit(
            System.Windows.ExitEventArgs e)
        {
            try
            {
                if (ownsSingleInstanceMutex)
                {
                    singleInstanceMutex?.ReleaseMutex();
                }

                singleInstanceMutex?.Dispose();
            }
            catch (Exception ex)
            {
                Services.LoggerService.Log($"Single instance cleanup error: {ex.Message}");
            }

            base.OnExit(e);
        }
    }
}
