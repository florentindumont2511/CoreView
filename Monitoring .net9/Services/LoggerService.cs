using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Monitoring_net9.Services
{
    public static class LoggerService
    {
        private static readonly string logPath =
            "monitoring_log.txt";

        public static void Log(string message)
        {
            try
            {
                File.AppendAllText(
                    logPath,
                    $"[{DateTime.Now}] {message}\n");
            }
            catch
            {
            }
        }
    }
}