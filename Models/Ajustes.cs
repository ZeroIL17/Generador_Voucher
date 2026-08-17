using System;
using System.IO;
using System.Text.Json;

namespace GeneradorVoucher_MP.Models
{
    public static class Ajustes
    {
        private static string GetSettingsPath()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GeneradorVoucher_MP");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "settings.json");
        }

        public static TasasConversion GetAppSettings()
        {
            var path = GetSettingsPath();
            if (!File.Exists(path))
                return new TasasConversion();

            try
            {
                var json = File.ReadAllText(path);
                var s = JsonSerializer.Deserialize<TasasConversion>(json);
                return s ?? new TasasConversion();
            }
            catch
            {
                return new TasasConversion();
            }
        }

        public static void Save(TasasConversion settings)
        {
            var path = GetSettingsPath();
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
