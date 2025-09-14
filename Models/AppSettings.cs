﻿using System.IO;
using System.Globalization;

namespace Passwd_VaultManager.Models
{
    public static class AppSettings
    {
        public static bool SoundEnabled { get; set; } = true;
        public static string FontFamily { get; set; } = "Segoe UI";
        public static double FontSize { get; set; } = 16.5;

        public static void LoadFromFile(string iniPath) {
            if (!File.Exists(iniPath)) return;

            foreach (var raw in File.ReadAllLines(iniPath)) {
                var line = raw.Trim();
                if (line.Length == 0) continue;                 // skip blanks
                if (line.StartsWith("#") || line.StartsWith(";") // skip comments
                    || line.StartsWith("//")) continue;

                var parts = line.Split('=', 2);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim().ToLowerInvariant();
                var value = parts[1].Trim().Trim('"'); // strip optional quotes

                switch (key) {
                    case "soundenabled":
                        SoundEnabled = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                        break;

                    case "fontfamily":
                        FontFamily = value;
                        break;

                    case "fontsize":
                        if (!TryParseDoubleAny(value, out var size))
                            break;
                        FontSize = size;
                        break;
                }
            }
        }

        private static bool TryParseDoubleAny(string s, out double result) {
            // invariant first
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return true;

            // current culture
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out result))
                return true;

            // naive fallback: swap comma/dot
            var swapped = s.Replace(',', '.');
            return double.TryParse(swapped, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        public static void SaveToFile(string iniPath) {
            var dir = Path.GetDirectoryName(iniPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var lines = new[]
            {
                $"FontFamily={FontFamily}",
                $"FontSize={FontSize.ToString(CultureInfo.InvariantCulture)}",
                $"SoundEnabled={(SoundEnabled ? "true" : "false")}"
            };
            File.WriteAllLines(iniPath, lines);
        }
    }
}
