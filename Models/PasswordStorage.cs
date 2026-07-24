using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GeneradorVoucher_MP.Models
{
    public static class PasswordStorage
    {
        // Apunta automáticamente a AppData en Windows y ~/.config en macOS
        private static readonly string FolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GeneradorVoucher"
        );
        private static readonly string FilePath = Path.Combine(FolderPath, "db_key.dat");

        public static bool ExistePassword() => File.Exists(FilePath);

        public static void GuardarPassword(string password)
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            File.WriteAllText(FilePath, password.Trim());
        }

        public static string? ObtenerPassword()
        {
            if (!ExistePassword()) return null;
            return File.ReadAllText(FilePath).Trim();
        }

        public static void BorrarPassword()
        {
            if (ExistePassword()) File.Delete(FilePath);
        }
    }
}
