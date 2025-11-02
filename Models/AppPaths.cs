using System.IO;

namespace Passwd_VaultManager.Models {
    public static class AppPaths {
        public static string AppDataFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PasswordVaultManager");

        public static string DatabaseFile => Path.Combine(AppDataFolder, "Vault.db");
        public static string SettingsFile => Path.Combine(AppDataFolder, "settings.ini");
        public static string BackupFolder => Path.Combine(AppDataFolder, "Backups");
        public static string PinFile => Path.Combine(AppDataFolder, "pin.dat");

        public static void EnsureAppDataFolder() {
            Directory.CreateDirectory(AppDataFolder);
            Directory.CreateDirectory(BackupFolder);
        }
    }
}