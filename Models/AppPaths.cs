using System.IO;

namespace Passwd_VaultManager.Models
{
    public static class AppPaths
    {
        public static void InitializeSettingsFile() {
            if (!File.Exists(SettingsFile)) {
                var defaults = new Dictionary<string, string> {
                    ["FontFamily"] = "Segoe UI",
                    ["FontSize"] = "14",
                    ["SoundEnabled"] = "true"
                };

                using var writer = new StreamWriter(SettingsFile);
                foreach (var kvp in defaults)
                    writer.WriteLine($"{kvp.Key}={kvp.Value}");
            }
        }

        // Base app data directory: %APPDATA%\DreamRecorder
        public static string AppDataFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PasswordVaultManager");

        // Database path
        public static string DatabaseFile => Path.Combine(AppDataFolder, "Vault.db");

        // Settings file path
        public static string SettingsFile => Path.Combine(AppDataFolder, "settings.ini");

        // Optional: backup location
        public static string BackupFolder => Path.Combine(AppDataFolder, "Backups");

        // Check if app is running for the first time
        public static bool IsFirstRun => !File.Exists(SettingsFile);

        // Ensure folders exist before use
        public static void EnsureAppDataFolder() {
            Directory.CreateDirectory(AppDataFolder);
            Directory.CreateDirectory(BackupFolder);
        }

        // Save or update a single setting (overwrites existing key)
        public static void SaveSetting(string key, string value) {
            EnsureAppDataFolder();

            var settings = LoadSettings();
            settings[key] = value;

            using var writer = new StreamWriter(SettingsFile);
            foreach (var kvp in settings) {
                writer.WriteLine($"{kvp.Key}={kvp.Value}");
            }
        }

        // Load settings from file into dictionary
        public static Dictionary<string, string> LoadSettings() {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (File.Exists(SettingsFile)) {
                foreach (var line in File.ReadAllLines(SettingsFile)) {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                        result[parts[0].Trim()] = parts[1].Trim();
                }
            }

            return result;
        }

        // Try get a setting value safely
        public static bool TryGetSetting(string key, out string value) {
            var settings = LoadSettings();
            return settings.TryGetValue(key, out value);
        }

        public static bool SoundEnabled {
            get {
                return TryGetSetting("SoundEnabled", out string value)
                       && value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static string PinFile => Path.Combine(AppDataFolder, "pin.dat");
    }
}
