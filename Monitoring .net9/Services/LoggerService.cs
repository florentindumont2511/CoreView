using System.IO;

namespace Monitoring_net9.Services
{
    public static class LoggerService
    {
        private const long MaxLogSizeBytes = 1024 * 1024;
        private const int ArchiveCount = 3;
        private static readonly TimeSpan DuplicateWindow = TimeSpan.FromSeconds(30);
        private static readonly object SyncRoot = new();
        private static readonly Dictionary<string, LogEntryState> RecentMessages = [];

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
                    DateTime now = DateTime.Now;

                    if (!TryPrepareMessage(message, now, out string preparedMessage))
                    {
                        return;
                    }

                    Directory.CreateDirectory(LogDirectory);
                    RotateLogIfNeeded();

                    File.AppendAllText(
                        LogPath,
                        $"[{now:yyyy-MM-dd HH:mm:ss.fff}] {preparedMessage}\n");
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

            for (int index = ArchiveCount; index >= 2; index--)
            {
                string olderArchive = GetArchivePath(index - 1);
                string newerArchive = GetArchivePath(index);

                if (File.Exists(olderArchive))
                {
                    File.Move(olderArchive, newerArchive, true);
                }
            }

            File.Move(LogPath, GetArchivePath(1), true);
        }

        private static bool TryPrepareMessage(
            string message,
            DateTime now,
            out string preparedMessage)
        {
            preparedMessage = message;

            if (RecentMessages.TryGetValue(message, out LogEntryState? state))
            {
                if (now - state.LastWritten < DuplicateWindow)
                {
                    state.SuppressedCount++;
                    return false;
                }

                if (state.SuppressedCount > 0)
                {
                    preparedMessage =
                        $"{message} (répété {state.SuppressedCount + 1} fois)";
                }

                state.LastWritten = now;
                state.SuppressedCount = 0;
            }
            else
            {
                RecentMessages[message] = new LogEntryState(now);
            }

            if (RecentMessages.Count > 250)
            {
                foreach (string oldMessage in RecentMessages
                             .Where(item => now - item.Value.LastWritten > TimeSpan.FromHours(1))
                             .Select(item => item.Key)
                             .ToArray())
                {
                    RecentMessages.Remove(oldMessage);
                }
            }

            return true;
        }

        private static string GetArchivePath(int index)
        {
            return Path.Combine(
                LogDirectory,
                $"monitoring_log.{index}.txt");
        }

        private sealed class LogEntryState(DateTime lastWritten)
        {
            public DateTime LastWritten { get; set; } = lastWritten;

            public int SuppressedCount { get; set; }
        }
    }
}
