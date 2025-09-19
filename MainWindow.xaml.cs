﻿using Hardcodet.Wpf.TaskbarNotification;
using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Properties;
using Passwd_VaultManager.ViewModels;
using System.Windows;

namespace Passwd_VaultManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //private readonly PasswdPanelVM _vm = new();
        private readonly MainWindowVM _vm = new();

        public MainWindow()
        {
            InitializeComponent();
            //DataContext = _vm;

            DataContext = _vm;
        }

        private void frmMainWindow_Loaded(object sender, RoutedEventArgs e) {

            // Startups...
            AppPaths.EnsureAppDataFolder();     // Make sure AppData folder exists
            DatabaseHandler.initDatabase();     // Ensure Database exists, if not, create it.

            // 1. Get vault records from DB

            //1. DB sim.
            //ObservableCollection<AppVault> testList = _vm.GetAppVaults();

            //testList.Add(new AppVault { AppName = "Gmail", UserName = "TestUsername1", Password = "1234", IsPasswdSet = true, IsUserNameSet = true, IsStatusGood = true });
            //testList.Add(new AppVault { AppName = "someMail", UserName = "TestUsername2", Password = "1234", IsPasswdSet = true, IsUserNameSet = false, IsStatusGood = false });
            //testList.Add(new AppVault { AppName = "WorkMail", UserName = "TestUsername3", Password = "1234", IsPasswdSet = false, IsUserNameSet = true, IsStatusGood = true });

            //_vm.SelectedAppVault = testList[0];



            // 2. load them into the panel.
            //foreach (var v in testList) {
            //    itms_PasswdPanelList.Items.Add(v);
            //}
        }

        private void cmdClose_Click(object sender, RoutedEventArgs e) {
            CloseWindowHandler();   // Call handler to minimise to system tray.
        }

        

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

        }

        private void NewVaultRecord_Click(object sender, RoutedEventArgs e) {

        }

        private void EditVaultRecord_Click(object sender, RoutedEventArgs e) {

        }

        private void DeleteVaultRecord_Click(object sender, RoutedEventArgs e) {

        }

        private void ExitApp_Click(object sender, RoutedEventArgs e) {
            CloseWindowHandler();   // Call handler to minimise to system tray.
        }




        /// <summary>
        /// Minimizes the main application window and displays a notification if the application is minimized to the
        /// system tray for the first time.
        /// </summary>
        /// <remarks>This method hides the main application window and, if a system tray icon is
        /// available, shows a balloon tip notification to inform the user that the application is still running in the
        /// background. The notification is displayed only once per application session, based on the <see
        /// cref="Settings.Default.TrayTipShown"/> setting.</remarks>
        private void CloseWindowHandler() {
            var mainWindow = Application.Current.MainWindow;
            mainWindow?.Hide();

            if (!Settings.Default.TrayTipShown &&
                TryFindResource("TrayIcon") is Hardcodet.Wpf.TaskbarNotification.TaskbarIcon trayIcon) {
                trayIcon.ShowBalloonTip(
                    "Minimized to Tray",
                    "Daily Reminder is still running in the background.",
                    BalloonIcon.Info
                );

                Settings.Default.TrayTipShown = true;
                Settings.Default.Save();
                System.Diagnostics.Debug.WriteLine("TrayTipShown: " + Settings.Default.TrayTipShown);

            }
        }
    }
}