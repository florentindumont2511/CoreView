using Monitoring_net9.Models;
using System.IO;
using System.Text.Json;

namespace Monitoring_net9.Services
{
    public static class SettingsService
    {
        private static readonly string filePath =
            "settings.json";

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new AppSettings();
                }

                string json =
                    File.ReadAllText(filePath);

                return JsonSerializer.Deserialize<AppSettings>(json)
                       ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public static void Save(
            AppSettings settings)
        {
            string json =
                JsonSerializer.Serialize(
                    settings,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(filePath, json);
        }
    }
}