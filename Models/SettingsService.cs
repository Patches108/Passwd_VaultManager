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
                ["FirstTimeOpeningNewWin"] = (s, v) => s.FirstTimeOpeningNewWin = v.Equals("true", StringComparison.OrdinalIgnoreCase),
                ["FirstTimeOpeningEditWin"] = (s, v) => s.FirstTimeOpeningEditWin = v.Equals("true", StringComparison.OrdinalIgnoreCase),

                ["FirstTimeNewAppName_NewWin"] = (s, v) => s.FirstTimeNewAppName_NewWin = v.Equals("true", StringComparison.OrdinalIgnoreCase),
                ["FirstTimeNewUserName_NewWin"] = (s, v) => s.FirstTimeNewUserName_NewWin = v.Equals("true", StringComparison.OrdinalIgnoreCase),
                ["FirstTimeNewPassword_NewWin"] = (s, v) => s.FirstTimeNewPassword_NewWin = v.Equals("true", StringComparison.OrdinalIgnoreCase),

                ["FirstTimeNewAppName_EditWin"] = (s, v) => s.FirstTimeNewAppName_EditWin = v.Equals("true", StringComparison.OrdinalIgnoreCase),
                ["FirstTimeNewUserName_EditWin"] = (s, v) => s.FirstTimeNewUserName_EditWin = v.Equals("true", StringComparison.OrdinalIgnoreCase),
                ["FirstTimeNewPassword_EditWin"] = (s, v) => s.FirstTimeNewPassword_EditWin = v.Equals("true", StringComparison.OrdinalIgnoreCase),
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
                $"FirstTimeOpeningNewWin={(s.FirstTimeOpeningNewWin ? "true" : "false")}",
                $"FirstTimeOpeningEditWin={(s.FirstTimeOpeningEditWin ? "true" : "false")}",

                $"FirstTimeNewAppName_NewWin={(s.FirstTimeNewAppName_NewWin ? "true" : "false")}",
                $"FirstTimeNewUserName_NewWin={(s.FirstTimeNewUserName_NewWin ? "true" : "false")}",
                $"FirstTimeNewPassword_NewWin={(s.FirstTimeNewPassword_NewWin ? "true" : "false")}",

                $"FirstTimeNewAppName_EditWin={(s.FirstTimeNewAppName_EditWin ? "true" : "false")}",
                $"FirstTimeNewUserName_EditWin={(s.FirstTimeNewUserName_EditWin ? "true" : "false")}",
                $"FirstTimeNewPassword_EditWin={(s.FirstTimeNewPassword_EditWin ? "true" : "false")}",
            };
            File.WriteAllLines(path, lines);
        }

        private static void InitializeDefaultsFile(string path) {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var defaults = new[] {

                "FontFamily=Segoe UI",
                "FontSize=16.5",
                "SoundEnabled=true",
                "FirstTimeOpeningApp=true",
                "FirstTimeOpeningNewWin=true",
                "FirstTimeOpeningEditWin=true",

                "FirstTimeNewAppName_NewWin=true",
                "FirstTimeNewUserName_NewWin=true",
                "FirstTimeNewPassword_NewWin=true",

                "FirstTimeNewAppName_EditWin=true",
                "FirstTimeNewUserName_EditWin=true",
                "FirstTimeNewPassword_EditWin=true"
            };
            File.WriteAllLines(path, defaults);
        }

        public static string ResetHelperBot(AppSettings s) {

            try {
                AppPaths.EnsureAppDataFolder();
                var path = AppPaths.SettingsFile;

                var lines = new[] {
                $"FontFamily={s.FontFamily}",
                $"FontSize={s.FontSize.ToString(CultureInfo.InvariantCulture)}",
                $"SoundEnabled={(s.SoundEnabled ? "true" : "false")}",

                "FirstTimeOpeningApp=true",
                "FirstTimeOpeningNewWin=true",
                "FirstTimeOpeningEditWin=true",

                "FirstTimeNewAppName_NewWin=true",
                "FirstTimeNewUserName_NewWin=true",
                "FirstTimeNewPassword_NewWin=true",

                "FirstTimeNewAppName_EditWin=true",
                "FirstTimeNewUserName_EditWin=true",
                "FirstTimeNewPassword_EditWin=true"
            };
                File.WriteAllLines(path, lines);
            } catch (Exception e) {
                return e.Message;
            }

            return "";
            
        }
    }
}
