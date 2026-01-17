using Hardcodet.Wpf.TaskbarNotification;
using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Passwd_VaultManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static AppSettings Settings { get; private set; }

        private TaskbarIcon _trayIcon;

        protected override void OnStartup(StartupEventArgs e) {

            SQLitePCL.Batteries.Init();         // Initializes SQLitePCLRaw provider (native engine binding)

            base.OnStartup(e);

            // Startups...
            AppPaths.EnsureAppDataFolder();     // Make sure AppData folder exists
            Settings = SettingsService.Load();
            DatabaseHandler.initDatabase();     // Ensure Database exists, if not, create it.
            EncryptionService.Initialize();

            _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
            _trayIcon.TrayMouseDoubleClick += (_, _) => ShowMainWindow();
        }

        protected override void OnExit(ExitEventArgs e) {
            _trayIcon?.Dispose();
            base.OnExit(e);
        }

        private void TrayMenu_Open_Click(object sender, RoutedEventArgs e) => ShowMainWindow();

        private void TrayMenu_Exit_Click(object sender, RoutedEventArgs e) {
            _trayIcon?.Dispose();
            Shutdown();
        }

        private void ShowMainWindow() {
            if (Current.MainWindow is { } window) {
                window.Show();
                window.WindowState = WindowState.Normal;
                window.Activate();
            }
        }

        private void TrayMenu_New_Click(object sender, RoutedEventArgs e) {

            // OPEN NEW WINDOW.
            if (Application.Current.MainWindow is MainWindow main)
                main.OpenNewVaultWindow();

        }


        private void TrayMenu_Settings_Click(object sender, RoutedEventArgs e) {
            
            // OPEN SETTINGS WINDOW.
            if (Application.Current.MainWindow is MainWindow main)
                main.OpenSettingsWindow();

        }
    }

}
