using Microsoft.Win32;
using Passwd_VaultManager.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;

namespace Passwd_VaultManager.ViewModels
{
    /// <summary>
    /// ViewModel for the Settings window.
    /// 
    /// Exposes application configuration options such as font selection,
    /// font size, sound toggles, and Windows startup behavior.
    /// </summary>
    class SettingsWindowVM : ViewModelBase {

        private bool _isStartupEnabled;

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

        public bool IsStartupEnabled {
            get => _isStartupEnabled;
            set {
                if (_isStartupEnabled != value) {
                    _isStartupEnabled = value;
                    OnPropertyChanged();
                    RegisterInStartup(value);
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


        /// <summary>
        /// Initializes the settings ViewModel and ensures the application
        /// data directory exists.
        /// </summary>
        public SettingsWindowVM() {
            AppPaths.EnsureAppDataFolder();
        }


        /// <summary>
        /// Registers or unregisters the application to run at Windows startup
        /// by updating the current user's Run registry key.
        /// </summary>
        /// <param name="enable">
        /// <c>true</c> to enable startup registration; <c>false</c> to disable it.
        /// </param>
        public static void RegisterInStartup(bool enable) {
            string appName = "PasswordVaultManager";
            string exePath = Process.GetCurrentProcess().MainModule.FileName;

            using RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (enable) {
                rk.SetValue(appName, $"\"{exePath}\"");
            } else {
                rk.DeleteValue(appName, false);
            }
        }
    }
}
