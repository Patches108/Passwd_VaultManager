using System.Globalization;
using System.IO;
using Passwd_VaultManager.Models;

namespace Passwd_VaultManager.Services {
    public static class SettingsService {
        private static readonly Dictionary<string, Action<AppSettings, string>> _setters =
            new(StringComparer.OrdinalIgnoreCase) {
                ["soundenabled"] = (s, v) => s.SoundEnabled = v.Equals("true", StringComparison.OrdinalIgnoreCase),
                ["fontfamily"] = (s, v) => s.FontFamily = v,
                ["fontsize"] = (s, v) => { if (TryParseDoubleAny(v, out var d)) s.FontSize = d; },
                ["firsttimeopeningapp"] = (s, v) => s.FirstTimeOpeningApp = v.Equals("true", StringComparison.OrdinalIgnoreCase),
            };

        private static bool TryParseDoubleAny(string s, out double result) {
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result)) return true;
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out result)) return true;
            var swapped = s.Replace(',', '.'); // naive fallback
            return double.TryParse(swapped, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        public static AppSettings Load() {
            //AppPaths.EnsureAppDataFolder();
            var path = AppPaths.SettingsFile;

            var settings = new AppSettings();

            if (!File.Exists(path)) {
                InitializeDefaultsFile(path);
                return settings; // defaults
            }

            foreach (var raw in File.ReadAllLines(path)) {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith(";") || line.StartsWith("//")) continue;

                var parts = line.Split('=', 2);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim().Trim('"');

                if (_setters.TryGetValue(key, out var apply)) {
                    apply(settings, value);
                }
            }

            return settings;
        }

        public static void Save(AppSettings s) {
            AppPaths.EnsureAppDataFolder();
            var path = AppPaths.SettingsFile;

            var lines = new[] {
                $"FontFamily={s.FontFamily}",
                $"FontSize={s.FontSize.ToString(CultureInfo.InvariantCulture)}",
                $"SoundEnabled={(s.SoundEnabled ? "true" : "false")}",
                $"FirstTimeOpeningApp={(s.FirstTimeOpeningApp ? "true" : "false")}",
            };
            File.WriteAllLines(path, lines);
        }

        private static void InitializeDefaultsFile(string path) {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var defaults = new[] {
                "FontFamily=Segoe UI",
                "FontSize=16.5",
                "SoundEnabled=true",
                "FirstTimeOpeningApp=true"
            };
            File.WriteAllLines(path, defaults);
        }
    }
}
