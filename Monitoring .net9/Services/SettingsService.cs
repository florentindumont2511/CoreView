using Monitoring_net9.Models;
using System.IO;
using System.Text.Json;

namespace Monitoring_net9.Services
{
    public static class SettingsService
    {
        private static readonly string SettingsDirectory =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "CoreView");

        private static readonly string FilePath =
            Path.Combine(SettingsDirectory, "settings.json");

        private static readonly string LegacyFilePath =
            "settings.json";

        public static AppSettings Load()
        {
            try
            {
                MigrateLegacySettings();

                if (!File.Exists(FilePath))
                {
                    return new AppSettings();
                }

                string json =
                    File.ReadAllText(FilePath);

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
            Directory.CreateDirectory(SettingsDirectory);

            string json =
                JsonSerializer.Serialize(
                    settings,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(FilePath, json);
        }

        private static void MigrateLegacySettings()
        {
            if (File.Exists(FilePath) || !File.Exists(LegacyFilePath))
            {
                return;
            }

            Directory.CreateDirectory(SettingsDirectory);
            File.Copy(LegacyFilePath, FilePath);
        }
    }
}
