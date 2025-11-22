using Hardcodet.Wpf.TaskbarNotification;
using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Properties;
using Passwd_VaultManager.Services;
using Passwd_VaultManager.ViewModels;
using Passwd_VaultManager.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Passwd_VaultManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowVM _vm = new();

        private static readonly string PlaceholderText = "Search by App Name";

        private static readonly Brush PlaceholderBrush = new SolidColorBrush(Color.FromRgb(140, 140, 140));
        private static readonly Brush InputBrush = (Brush)new BrushConverter().ConvertFromString("#FF2BA33B");

        internal MainWindowVM ViewModel { get; set; }

        internal MainWindowVM MainVM => this.DataContext as MainWindowVM;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;

            txtSearch.Text = "Search by App Name";
            txtSearch.Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140));

            var sharedPasswdPanelVM = (PasswdPanelVM)Resources["PasswdPanelVM"];
            _vm.PasswdPanelVM = sharedPasswdPanelVM;  // store it inside your MainWindowVM
            DataContext = _vm;

            //App.Settings.FirstTimeOpeningApp = true;       // REMOVE THIS IN PROD

            if (App.Settings.FirstTimeOpeningApp) {
                var helpWin = new Helper("I see this is your first time using Password Vault Manager.\n\nWelcome!\n\nTo get started, click the \'New\'");
                helpWin.Show();
                App.Settings.FirstTimeOpeningApp = false;
                SettingsService.Save(App.Settings);
            }
        }        

        private async void frmMainWindow_Loaded(object sender, RoutedEventArgs e) {

            // Now gate access with PIN
            if (PinStorage.HasPin()) {
                var dlg = new pin();
                bool? ok = dlg.ShowDialog();
                if (ok != true) return; // user canceled/failed → app closes (by your window)
            }

            await _vm.RefreshVaultsAsync();
        }

        private void cmdClose_Click(object sender, RoutedEventArgs e) {
            CloseWindowHandler();   // Call handler to minimise to system tray.
        }
                

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

        }

        private void NewVaultRecord_Click(object sender, RoutedEventArgs e) {
            OpenNewVaultWindow();
        }

        private void EditVaultRecord_Click(object sender, RoutedEventArgs e) {
            
            AppVault vault =  _vm.SelectedAppVault; // Get selected vault
            if (vault is null) {
                new Helper("First, select a vault to edit.").Show();
                return;
            }

            var vm = new EditWindowVM(vault);
            var win = new EditWindow { DataContext = vm };
            win.ShowDialog();
        }

        private void DeleteVaultRecord_Click(object sender, RoutedEventArgs e) {
            AppVault vault = _vm.SelectedAppVault; // Get selected vault
            if (vault is null) {
                new Helper("First, select a vault to delete.").Show();
                return;
            }

            _vm.DeleteVaultEntryCommand.Execute(vault);
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown(); // Hard exit.
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

        //private void cmd_OpenNewVaultWindow_Click(object sender, RoutedEventArgs e) {

        //    // pass in _refreshAction
        //    //var shared = (NewWindowVM)Resources["NewWindowVM"];
        //    //var win = new NewWindow { DataContext = shared }; // use SAME instance
        //    //win.Show();
        //}

        private void ListBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape && sender is ListBox lb) {
                lb.SelectedItem = null;
                e.Handled = true;
            }
        }


        public void OpenNewVaultWindow() {
            Func<Task> _refreshAction;
            _refreshAction = _vm.RefreshVaultsAsync;
            var win = new NewWindow(_refreshAction);
            win.Show();
        }


        public void OpenSettingsWindow() {
            var win = new SettingsWindow();
            win.Show();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e) {
            OpenSettingsWindow();
        }

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e) {
            if (txtSearch.Text == PlaceholderText) {
                txtSearch.Text = "";
                txtSearch.Foreground = InputBrush;
            }
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(txtSearch.Text)) {
                txtSearch.Text = PlaceholderText;
                txtSearch.Foreground = PlaceholderBrush;
            }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            var closeBtn = GetTemplateChild("btnClose") as Button;
            if (closeBtn != null) {
                closeBtn.Click += (s, e) => this.Close();
            }

            var minimizeBtn = GetTemplateChild("btnMinimize") as Button;
            if (minimizeBtn != null) {
                minimizeBtn.Click += (s, e) => this.WindowState = WindowState.Minimized;
            }
        }

        private void FilterMenu_IsPasswdSet_Click(object sender, RoutedEventArgs e) {

        }

        private void FilterMenu_IsUserNameSet_Click(object sender, RoutedEventArgs e) {

        }

        private void FilterMenu_BitRate265_Click(object sender, RoutedEventArgs e) {

        }

        private void FilterMenu_BitRate192_Click(object sender, RoutedEventArgs e) {

        }

        private void FilterMenu_BitRate128OrLess_Click(object sender, RoutedEventArgs e) {

        }

        private void FilterMenu_StatusGood_Click(object sender, RoutedEventArgs e) {

        }

        private void FilterMenu_Cancel_Click(object sender, RoutedEventArgs e) {

        }

        private void cmdFilter_Click(object sender, RoutedEventArgs e) {
            Button btn = (Button)sender;

            if (btn.ContextMenu != null) {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

    }
}