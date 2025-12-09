using Hardcodet.Wpf.TaskbarNotification;
using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Properties;
using Passwd_VaultManager.Services;
using Passwd_VaultManager.ViewModels;
using Passwd_VaultManager.Views;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        private bool _sortedFlag = false;
        private bool _filteredFlag = false;

        private Func<Task> _refreshAction;

        private static readonly Brush PlaceholderBrush = new SolidColorBrush(Color.FromRgb(140, 140, 140));
        private static readonly Brush InputBrush = (Brush)new BrushConverter().ConvertFromString("#FF2BA33B");
        private static readonly Brush DefaultBorderBrush = (Brush)new BrushConverter().ConvertFrom("#FF1F89F6");   // default blue
        private static readonly Brush MatchBorderBrush = Brushes.LimeGreen;
        private static readonly Brush NoResultBorderBrush = Brushes.Red;

        private ObservableCollection<AppVault> SearchedVaults = new ObservableCollection<AppVault>();

        internal MainWindowVM ViewModel { get; set; }

        internal MainWindowVM MainVM => this.DataContext as MainWindowVM;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;

            txtSearch.Text = "Search by App Name";
            txtSearch.Foreground = new SolidColorBrush(Color.FromRgb(140, 140, 140));

            var sharedPasswdPanelVM = (PasswdPanelVM)Resources["PasswdPanelVM"];
            _vm.PasswdPanelVM = sharedPasswdPanelVM;  // store inside MainWindowVM
            DataContext = _vm;

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
            _refreshAction = _vm.RefreshVaultsAsync;  //set delegate to refresh vaults on main window.
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
            
            AppVault vault = _vm.SelectedAppVault; // Get selected vault
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
            //Func<Task> _refreshAction;
            //_refreshAction = _vm.RefreshVaultsAsync;
            var win = new NewWindow(_refreshAction);
            win.Show();
        }


        public void OpenSettingsWindow() {
            var win = new SettingsWindow(_refreshAction);
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
            FilterHandler("IsPasswdSet");
        }

        private void FilterMenu_IsUserNameSet_Click(object sender, RoutedEventArgs e) {
            FilterHandler("IsUserNameSet");
        }

        private void FilterMenu_BitRate265_Click(object sender, RoutedEventArgs e) {
            FilterHandler("BitRate265");
        }

        private void FilterMenu_BitRate192_Click(object sender, RoutedEventArgs e) {
            FilterHandler("BitRate192");
        }

        private void FilterMenu_BitRate128OrLess_Click(object sender, RoutedEventArgs e) {
            FilterHandler("BitRate128");
        }

        private void FilterMenu_StatusGood_Click(object sender, RoutedEventArgs e) {
            FilterHandler("StatusGood");
        }
        private void FilterMenu_StatusBad_Click(object sender, RoutedEventArgs e) {
            FilterHandler("StatusBad");
        }
        private void FilterMenu_RemoveFilter_Click(object sender, RoutedEventArgs e) {
            FilterHandler("RemoveFilter");
        }
        private void FilterMenu_Reset_Click(object sender, RoutedEventArgs e) {
            FilterHandler("Reset");
        }

        private void FilterMenu_Cancel_Click(object sender, RoutedEventArgs e) {
            // NOTHING HERE, IT WILL JUST CLOSE.
        }

        private void cmdFilter_Click(object sender, RoutedEventArgs e) {
            Button btn = (Button)sender;

            if (btn.ContextMenu != null) {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
            string searchText = txtSearch.Text;

            // 1. Empty or whitespace -> reset
            if (string.IsNullOrWhiteSpace(searchText) || txtSearch.Text.Equals("Search by App Name")) {
                txtSearch.BorderBrush = DefaultBorderBrush;

                // ensure vault list is up to date
                await _vm.RefreshVaultsAsync();
                lstVaultList.ItemsSource = _vm.Vaults;

                return;
            }

            // 2. Make sure we have the latest vaults once per change
            await _vm.RefreshVaultsAsync();
            SearchedVaults = _vm.Vaults ?? new ObservableCollection<AppVault>();

            string search = searchText.Trim();

            // 3. Case-insensitive, null-safe search in memory
            var searchedList = SearchedVaults
                .Where(v => !string.IsNullOrEmpty(v.AppName) &&
                            v.AppName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            // 4. Update UI + border color based on results
            if (searchedList.Count > 0) {
                txtSearch.BorderBrush = MatchBorderBrush;
                lstVaultList.ItemsSource = searchedList;
            } else {
                txtSearch.BorderBrush = NoResultBorderBrush;

                //lstVaultList.ItemsSource = allVaults;
                // Empty list.
                lstVaultList.ItemsSource = new ObservableCollection<AppVault>();
            }
        }


        private async void FilterHandler(string s) {
            // get all vaults
            //await _vm.RefreshVaultsAsync();

            var searchedList = new ObservableCollection<AppVault>();

            foreach (AppVault v in lstVaultList.Items) {
                searchedList.Add(v);
            }

            if(_filteredFlag) {
                _vm.PreFilteredVaults = searchedList;         // set state.
                _filteredFlag = true;
            }
            //var allVaults = _vm.Vaults ?? new ObservableCollection<AppVault>();
            //ObservableCollection<AppVault> searchedList = allVaults;

            switch (s) {
                case "IsPasswdSet":
                    searchedList = new ObservableCollection<AppVault>(searchedList.Where(v => v.IsPasswdSet));
                    break;

                case "IsUserNameSet":
                    searchedList = new ObservableCollection<AppVault>(searchedList.Where(v => v.IsUserNameSet));
                    break;

                case "BitRate265":
                    searchedList = new ObservableCollection<AppVault>(searchedList.Where(v => v.BitRate == 256));
                    break;

                case "BitRate192":
                    searchedList = new ObservableCollection<AppVault>(searchedList.Where(v => v.BitRate == 192));
                    break;

                case "BitRate128":
                    searchedList = new ObservableCollection<AppVault>(searchedList.Where(v => v.BitRate == 128));
                    break;

                case "StatusGood":
                    searchedList = new ObservableCollection<AppVault>(searchedList.Where(v => v.IsStatusGood));
                    break;

                case "StatusBad":
                    searchedList = new ObservableCollection<AppVault>(searchedList.Where(v => !v.IsStatusGood));
                    break;

                case "RemoveFilter":

                    if(_vm.PreFilteredVaults != null) {
                        searchedList = new ObservableCollection<AppVault>(_vm.PreFilteredVaults);
                        _filteredFlag = false;
                    }

                    break;

                case "Reset":
                    await _vm.RefreshVaultsAsync();
                    searchedList = _vm.Vaults;
                    //lstVaultList.ItemsSource = _vm.Vaults;
                    //searchedList.Clear();
                    _filteredFlag = false;

                    // if search field contains something, reset it.
                    if (txtSearch.Text.Trim() == PlaceholderText || !string.IsNullOrWhiteSpace(txtSearch.Text.Trim())) {
                        string txt = txtSearch.Text.Trim();
                        txtSearch.Text = string.Empty;
                        txtSearch.Text = txt;
                    }

                    return;

                default:
                    new MessageWindow("INTERNAL ERROR: Unknown Option");
                    return;
            }

            SearchedVaults = searchedList;
            lstVaultList.ItemsSource = SearchedVaults;
        }

        private void SortMenu_Alphabetical_Click(object sender, RoutedEventArgs e) {
            SortFunc("Alphabetical");
        }

        private void SortMenu_DateCreated_Click(object sender, RoutedEventArgs e) {
            SortFunc("DateCreated");
        }

        private void SortMenu_Bitrate_Click(object sender, RoutedEventArgs e) {
            SortFunc("Bitrate");
        }

        private void SortMenu_Status_Click(object sender, RoutedEventArgs e) {
            SortFunc("Status");
        }

        private void SortMenu_Reset_Click(object sender, RoutedEventArgs e) {
            SortFunc("Reset");
        }

        private void SortMenu_RemoveSort_Click(object sender, RoutedEventArgs e) {
            SortFunc("RemoveSort");
        }

        private void SortMenu_Cancel_Click(object sender, RoutedEventArgs e) {
            SortFunc("Cancel");
        }

        private async void SortFunc(String s) {

            var searchedList = new ObservableCollection<AppVault>();

            foreach (AppVault v in lstVaultList.Items) {
                searchedList.Add(v);
            }

            if (!_sortedFlag) {
                _vm.PreSortedVaults = searchedList;         // set state.
                _sortedFlag = true;
            }

            switch (s) {
                case "Alphabetical":
                    searchedList = new ObservableCollection<AppVault>(searchedList.OrderBy(v => v.AppName));
                    break;
                case "DateCreated":
                    searchedList = new ObservableCollection<AppVault>(searchedList.OrderBy(v => v.DateCreated));
                    break;
                case "Bitrate":
                    searchedList = new ObservableCollection<AppVault>(searchedList.OrderBy(v => v.BitRate));
                    break;
                case "Status":
                    searchedList = new ObservableCollection<AppVault>(searchedList.OrderBy(v => v.IsStatusGood));
                    break;
                case "Reset":
                    await _vm.RefreshVaultsAsync();
                    searchedList = _vm.Vaults;
                    //lstVaultList.ItemsSource = _vm.Vaults;
                    //searchedList.Clear();
                    _sortedFlag = false;

                    // if search field contains something, reset it.
                    if(txtSearch.Text.Trim() == PlaceholderText || !string.IsNullOrWhiteSpace(txtSearch.Text.Trim())) {
                        string txt = txtSearch.Text.Trim();
                        txtSearch.Text = string.Empty;
                        txtSearch.Text = txt;
                    }

                    return;
                case "RemoveSort":
                    if(_vm.PreSortedVaults != null) {
                        searchedList = new ObservableCollection<AppVault>(_vm.PreSortedVaults);
                        _sortedFlag = false;
                    }
                    break;
                case "Cancel":
                    return;
                default:
                    new MessageWindow("INTERNAL ERROR: Unknown Option");
                    return;
            }

            SearchedVaults = searchedList;
            lstVaultList.ItemsSource = SearchedVaults;

        }

        private void cmdSort_Click(object sender, RoutedEventArgs e) {
            Button btn = (Button)sender;

            if (btn.ContextMenu != null) {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }        
    }
}