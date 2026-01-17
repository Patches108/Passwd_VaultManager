using Passwd_VaultManager.Models;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Passwd_VaultManager.Services {
    public static class SettingsService {
        // Keep keys consistent with what Save() writes.
        // Dictionary is case-insensitive, but using a single canonical style prevents future mistakes.
        private static readonly Dictionary<string, Action<AppSettings, string>> _setters =
            new(StringComparer.OrdinalIgnoreCase) {
                ["SoundEnabled"] = (s, v) => s.SoundEnabled = ParseBool(v, defaultValue: s.SoundEnabled),
                ["SpeechEnabled"] = (s, v) => s.SpeechEnabled = ParseBool(v, defaultValue: s.SpeechEnabled),                
                ["FontFamily"] = (s, v) => s.FontFamily = v,
                ["FontSize"] = (s, v) => {
                    if (TryParseDoubleAny(v, out var d))
                        s.FontSize = d;
                },

                ["FirstTimeOpeningApp"] = (s, v) => s.FirstTimeOpeningApp = ParseBool(v, s.FirstTimeOpeningApp),
                ["FirstTimeOpeningNewWin"] = (s, v) => s.FirstTimeOpeningNewWin = ParseBool(v, s.FirstTimeOpeningNewWin),
                ["FirstTimeOpeningEditWin"] = (s, v) => s.FirstTimeOpeningEditWin = ParseBool(v, s.FirstTimeOpeningEditWin),

                ["FirstTimeNewAppName_NewWin"] = (s, v) => s.FirstTimeNewAppName_NewWin = ParseBool(v, s.FirstTimeNewAppName_NewWin),
                ["FirstTimeNewUserName_NewWin"] = (s, v) => s.FirstTimeNewUserName_NewWin = ParseBool(v, s.FirstTimeNewUserName_NewWin),
                ["FirstTimeNewPassword_NewWin"] = (s, v) => s.FirstTimeNewPassword_NewWin = ParseBool(v, s.FirstTimeNewPassword_NewWin),

                ["FirstTimeNewAppName_EditWin"] = (s, v) => s.FirstTimeNewAppName_EditWin = ParseBool(v, s.FirstTimeNewAppName_EditWin),
                ["FirstTimeNewUserName_EditWin"] = (s, v) => s.FirstTimeNewUserName_EditWin = ParseBool(v, s.FirstTimeNewUserName_EditWin),
                ["FirstTimeNewPassword_EditWin"] = (s, v) => s.FirstTimeNewPassword_EditWin = ParseBool(v, s.FirstTimeNewPassword_EditWin),
            };

        public static AppSettings Load() {
            AppPaths.EnsureAppDataFolder();
            var path = AppPaths.SettingsFile;

            // Defaults come from AppSettings' own defaults.
            var settings = new AppSettings();

            if (!File.Exists(path)) {
                InitializeDefaultsFile(path);
                return settings;
            }

            foreach (var raw in File.ReadAllLines(path)) {
                var line = raw.Trim();
                if (line.Length == 0) continue;

                // Comments
                if (line.StartsWith("#", StringComparison.Ordinal) ||
                    line.StartsWith(";", StringComparison.Ordinal) ||
                    line.StartsWith("//", StringComparison.Ordinal))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = UnwrapQuotes(parts[1].Trim());

                if (_setters.TryGetValue(key, out var apply))
                    apply(settings, value);
            }

            return settings;
        }

        public static void Save(AppSettings s) {
            AppPaths.EnsureAppDataFolder();
            var path = AppPaths.SettingsFile;

            var lines = Serialize(s);
            WriteAllLinesAtomic(path, lines);
        }

        /// <summary>
        /// Resets "first time" helper flags and persists them.
        /// Returns empty string on success, otherwise returns the error message.
        /// </summary>
        public static string ResetHelperBot(AppSettings s) {
            try {
                s.FirstTimeOpeningApp = true;
                s.FirstTimeOpeningNewWin = true;
                s.FirstTimeOpeningEditWin = true;
                s.SpeechEnabled = true;

                s.FirstTimeNewAppName_NewWin = true;
                s.FirstTimeNewUserName_NewWin = true;
                s.FirstTimeNewPassword_NewWin = true;

                s.FirstTimeNewAppName_EditWin = true;
                s.FirstTimeNewUserName_EditWin = true;
                s.FirstTimeNewPassword_EditWin = true;

                Save(s);
                return string.Empty;
            } catch (Exception e) {
                return e.Message;
            }
        }

        // -----------------------------
        // Helpers
        // -----------------------------

        private static string[] Serialize(AppSettings s) =>
        [
            $"FontFamily={s.FontFamily}",
            $"FontSize={s.FontSize.ToString(CultureInfo.InvariantCulture)}",
            $"SoundEnabled={(s.SoundEnabled ? "true" : "false")}",
            $"SpeechEnabled={(s.SpeechEnabled ? "true" : "false")}",

            $"FirstTimeOpeningApp={(s.FirstTimeOpeningApp ? "true" : "false")}",
            $"FirstTimeOpeningNewWin={(s.FirstTimeOpeningNewWin ? "true" : "false")}",
            $"FirstTimeOpeningEditWin={(s.FirstTimeOpeningEditWin ? "true" : "false")}",

            $"FirstTimeNewAppName_NewWin={(s.FirstTimeNewAppName_NewWin ? "true" : "false")}",
            $"FirstTimeNewUserName_NewWin={(s.FirstTimeNewUserName_NewWin ? "true" : "false")}",
            $"FirstTimeNewPassword_NewWin={(s.FirstTimeNewPassword_NewWin ? "true" : "false")}",

            $"FirstTimeNewAppName_EditWin={(s.FirstTimeNewAppName_EditWin ? "true" : "false")}",
            $"FirstTimeNewUserName_EditWin={(s.FirstTimeNewUserName_EditWin ? "true" : "false")}",
            $"FirstTimeNewPassword_EditWin={(s.FirstTimeNewPassword_EditWin ? "true" : "false")}",
        ];

        private static void InitializeDefaultsFile(string path) {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            // Use the same serializer to avoid drift
            var defaults = new AppSettings {
                FontFamily = "Segoe UI",
                FontSize = 15,
                SoundEnabled = true,
                SpeechEnabled = true,

                FirstTimeOpeningApp = true,
                FirstTimeOpeningNewWin = true,
                FirstTimeOpeningEditWin = true,

                FirstTimeNewAppName_NewWin = true,
                FirstTimeNewUserName_NewWin = true,
                FirstTimeNewPassword_NewWin = true,

                FirstTimeNewAppName_EditWin = true,
                FirstTimeNewUserName_EditWin = true,
                FirstTimeNewPassword_EditWin = true
            };

            WriteAllLinesAtomic(path, Serialize(defaults));
        }

        private static void WriteAllLinesAtomic(string path, string[] lines) {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var tmp = path + ".tmp";

            // Write temp first
            File.WriteAllLines(tmp, lines);

            // Replace original atomically-ish
            // File.Replace is best on Windows when the target exists.
            if (File.Exists(path)) {
                // If you want a backup: path + ".bak"
                File.Replace(tmp, path, destinationBackupFileName: null);
            } else {
                File.Move(tmp, path);
            }
        }

        private static bool ParseBool(string v, bool defaultValue) {
            if (string.IsNullOrWhiteSpace(v))
                return defaultValue;

            v = v.Trim();

            // common forms
            if (v.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
            if (v.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;

            if (v.Equals("1", StringComparison.OrdinalIgnoreCase)) return true;
            if (v.Equals("0", StringComparison.OrdinalIgnoreCase)) return false;

            if (v.Equals("yes", StringComparison.OrdinalIgnoreCase)) return true;
            if (v.Equals("no", StringComparison.OrdinalIgnoreCase)) return false;

            if (v.Equals("on", StringComparison.OrdinalIgnoreCase)) return true;
            if (v.Equals("off", StringComparison.OrdinalIgnoreCase)) return false;

            return defaultValue;
        }

        private static string UnwrapQuotes(string s) {
            // Only unwrap if it’s a full wrap: "value"
            if (s.Length >= 2 && s[0] == '"' && s[^1] == '"')
                return s.Substring(1, s.Length - 2);

            return s;
        }

        private static bool TryParseDoubleAny(string s, out double result) {
            // Prefer exact formats first
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result)) return true;
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out result)) return true;

            // If it's "16,5" in some locales but current culture isn't set as expected:
            // Only swap comma->dot when there's NO dot present (avoids 1,234.56 => 1.234.56)
            if (s.Contains(',') && !s.Contains('.')) {
                var swapped = s.Replace(',', '.');
                return double.TryParse(swapped, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
            }

            result = 0;
            return false;
        }
    }
}