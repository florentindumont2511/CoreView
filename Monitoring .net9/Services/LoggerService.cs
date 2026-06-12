using System.IO;

namespace Monitoring_net9.Services
{
    public static class LoggerService
    {
        private const long MaxLogSizeBytes = 1024 * 1024 * 5;
        private static readonly object SyncRoot = new();

        private static readonly string LogDirectory =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "CoreView");

        private static readonly string LogPath =
            Path.Combine(LogDirectory, "monitoring_log.txt");

        public static void Log(string message)
        {
            try
            {
                lock (SyncRoot)
                {
                    Directory.CreateDirectory(LogDirectory);
                    RotateLogIfNeeded();

                    File.AppendAllText(
                        LogPath,
                        $"[{DateTime.Now}] {message}\n");
                }
            }
            catch
            {
            }
        }

        private static void RotateLogIfNeeded()
        {
            if (!File.Exists(LogPath))
            {
                return;
            }

            var logFile = new FileInfo(LogPath);

            if (logFile.Length < MaxLogSizeBytes)
            {
                return;
            }

            string archivePath =
                Path.Combine(LogDirectory, "monitoring_log.previous.txt");

            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
            }

            File.Move(LogPath, archivePath);
        }
    }
}
