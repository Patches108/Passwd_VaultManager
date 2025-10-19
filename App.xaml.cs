using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Services;
using System.Windows;

namespace Passwd_VaultManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static AppSettings Settings { get; private set; }

        protected override void OnStartup(StartupEventArgs e) {

            SQLitePCL.Batteries.Init();         // Initializes SQLitePCLRaw provider (native engine binding)

            base.OnStartup(e);

            // Startups...
            AppPaths.EnsureAppDataFolder();     // Make sure AppData folder exists
            Settings = SettingsService.Load();
            DatabaseHandler.initDatabase();     // Ensure Database exists, if not, create it.
            EncryptionService.Initialize();
        }

    }

}
