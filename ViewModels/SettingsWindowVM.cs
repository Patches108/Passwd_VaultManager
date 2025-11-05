using Passwd_VaultManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Passwd_VaultManager.ViewModels
{
    class SettingsWindowVM : ViewModelBase {
        // Font size options
        public ObservableCollection<double> AvailableFontSizes { get; } = new ObservableCollection<double>(Enumerable.Range(20, 29).Select(i => i / 2.0));

        private double _selectedFontSize = 16.5;

        public double SelectedFontSize {
            get => _selectedFontSize;
            set {
                if (_selectedFontSize != value) {
                    _selectedFontSize = value;
                    OnPropertyChanged();
                }
            }
        }

        // Font options
        public ObservableCollection<string> AvailableFonts { get; } =
            new ObservableCollection<string>(
                Fonts.SystemFontFamilies
                     .Select(f => f.Source)
                     .OrderBy(name => name)
                     .Distinct());

        private string _selectedFont;
        public string SelectedFont {
            get => _selectedFont;
            set {
                if (_selectedFont != value) {
                    _selectedFont = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _soundEnabled;
        public bool SoundEnabled {
            get => _soundEnabled;
            set {
                if (_soundEnabled != value) {
                    _soundEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public SettingsWindowVM() {
            AppPaths.EnsureAppDataFolder();

            // Font family: prefer "FontFamily", fall back to legacy "Font"
            //if (AppPaths.TryGetSetting("FontFamily", out string fontFamily) ||
            //    AppPaths.TryGetSetting("Font", out fontFamily))           // legacy key
            //{
            //    SelectedFont = fontFamily;
            //} else {
            //    // use what's currently applied app-wide if available, else default
            //    SelectedFont = AppSettings.FontFamily ?? "Segoe UI";
            //}

            //// Font size (parse invariant so "16.5" works regardless of locale)
            //if (AppPaths.TryGetSetting("FontSize", out string fontSize) &&
            //    double.TryParse(fontSize, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedSize)) {
            //    SelectedFontSize = parsedSize;
            //} else {
            //    SelectedFontSize = AppSettings.FontSize; // your global default
            //}

            //// Sound
            //if (AppPaths.TryGetSetting("SoundEnabled", out string sound))
            //    SoundEnabled = sound.Equals("true", StringComparison.OrdinalIgnoreCase);
            //else
            //    SoundEnabled = AppSettings.SoundEnabled;
        }

        public void SaveSettings() {
            //AppPaths.SaveSetting("FontFamily", SelectedFont);
            //AppPaths.SaveSetting("FontSize", SelectedFontSize.ToString(CultureInfo.InvariantCulture));
            //AppPaths.SaveSetting("SoundEnabled", SoundEnabled ? "true" : "false");
        }
    }
}
