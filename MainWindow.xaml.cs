using Hardcodet.Wpf.TaskbarNotification;
using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Properties;
using Passwd_VaultManager.ViewModels;
using Passwd_VaultManager.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Passwd_VaultManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowVM _vm = new();
                
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;

            var sharedPasswdPanelVM = (PasswdPanelVM)Resources["PasswdPanelVM"];
            _vm.PasswdPanelVM = sharedPasswdPanelVM;  // store it inside your MainWindowVM
            DataContext = _vm;
        }        

        private async void frmMainWindow_Loaded(object sender, RoutedEventArgs e) {
            await _vm.RefreshVaultsAsync();
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

        private void cmd_OpenNewVaultWindow_Click(object sender, RoutedEventArgs e) {

            // pass in _refreshAction
            //var shared = (NewWindowVM)Resources["NewWindowVM"];
            //var win = new NewWindow { DataContext = shared }; // use SAME instance
            //win.Show();
        }
    }
}