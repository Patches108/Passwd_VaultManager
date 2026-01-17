using Hardcodet.Wpf.TaskbarNotification;
using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Properties;
using Passwd_VaultManager.Services;
using Passwd_VaultManager.ViewModels;
using Passwd_VaultManager.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Passwd_VaultManager {
    public partial class MainWindow : Window {
        private readonly MainWindowVM _vm = new();

        private const string PlaceholderText = "Search by App Name";

        private bool _sortedActive;
        private bool _filteredActive;

        private Func<Task>? _refreshAction;

        private static readonly Brush PlaceholderBrush = new SolidColorBrush(Color.FromRgb(140, 140, 140));
        private static readonly Brush InputBrush = (Brush)new BrushConverter().ConvertFromString("#FF2BA33B");
        private static readonly Brush DefaultBorderBrush = (Brush)new BrushConverter().ConvertFrom("#FF1F89F6");
        private static readonly Brush MatchBorderBrush = Brushes.LimeGreen;
        private static readonly Brush NoResultBorderBrush = Brushes.Red;

        // View-only state: current list shown in ListBox
        private ObservableCollection<AppVault> _currentView = new();

        // Snapshots for RemoveSort/RemoveFilter (should live in view, not VM)
        private List<AppVault>? _preSortSnapshot;
        private List<AppVault>? _preFilterSnapshot;

        private ICollectionView _vaultsView = null!;
        private string _searchText = "";
        private readonly HashSet<string> _filters = new();
        private SortDescription? _sort;

        public MainWindow() {
            InitializeComponent();

            DataContext = _vm;

            // Placeholder init
            txtSearch.Text = PlaceholderText;
            txtSearch.Foreground = PlaceholderBrush;
            txtSearch.BorderBrush = DefaultBorderBrush;

            // First time help
            if (App.Settings.FirstTimeOpeningApp) {
                var helpWin = new Helper(
                    "I see this is your first time using Password Vault Manager.\n\nWelcome!\n\nTo get started, click the 'New' button.",
                    SoundController.InfoSound);
                helpWin.Show();

                App.Settings.FirstTimeOpeningApp = false;
                SettingsService.Save(App.Settings);
            }
        }

        private async void frmMainWindow_Loaded(object sender, RoutedEventArgs e) {
            if (PinStorage.HasPin()) {
                var dlg = new pin();
                bool? ok = dlg.ShowDialog();
                if (ok != true) return;
            }

            _refreshAction = _vm.RefreshVaultsAsync;

            await _vm.RefreshVaultsAsync();
            await _vm.ComposeStatusMSG();
            _vm.UpdateAllVaultStatus();

            // Create the view over the SAME ObservableCollection the ListBox binds to.
            _vaultsView = CollectionViewSource.GetDefaultView(_vm.Vaults);
            _vaultsView.Filter = VaultFilter;

            SharedFuncs.Apply(this, App.Settings);
        }

        public void ApplyFontsFromSettingsWin() {
            SharedFuncs.Apply(this, App.Settings);
        }

        // --------------------------
        // Window controls / tray
        // --------------------------
        private void cmdClose_Click(object sender, RoutedEventArgs e) => CloseWindowHandler();

        private void ExitApp_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void CloseWindowHandler() {
            Application.Current.MainWindow?.Hide();

            if (!Settings.Default.TrayTipShown &&
                TryFindResource("TrayIcon") is TaskbarIcon trayIcon) {
                trayIcon.ShowBalloonTip(
                    "Minimized to Tray",
                    "Password Vault Manager is still running in the background.",
                    BalloonIcon.Info);

                Settings.Default.TrayTipShown = true;
                Settings.Default.Save();
            }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            if (GetTemplateChild("btnClose") is Button closeBtn)
                closeBtn.Click += (_, __) => CloseWindowHandler();

            if (GetTemplateChild("btnMinimize") is Button minimizeBtn)
                minimizeBtn.Click += (_, __) => WindowState = WindowState.Minimized;
        }

        // --------------------------
        // New / Edit / Delete
        // --------------------------
        private void NewVaultRecord_Click(object sender, RoutedEventArgs e) => OpenNewVaultWindow();

        public void OpenNewVaultWindow() {
            if (_refreshAction is null) _refreshAction = _vm.RefreshVaultsAsync;
            var win = new NewWindow(_refreshAction);
            win.Show();
        }

        private void EditVaultRecord_Click(object sender, RoutedEventArgs e) {
            var vault = _vm.SelectedAppVault;
            if (vault is null) {
                new Helper("First, select a vault to edit.", SoundController.ErrorSound).Show();
                return;
            }

            _vm.EditVaultEntryCommand.Execute(vault);
        }

        private void DeleteVaultRecord_Click(object sender, RoutedEventArgs e) {
            var vault = _vm.SelectedAppVault;
            if (vault is null) {
                new Helper("First, select a vault to delete.", SoundController.ErrorSound).Show();
                return;
            }

            _vm.DeleteVaultEntryCommand.Execute(vault);
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e) {
            var win = new SettingsWindow(_vm.RefreshVaultsAsync);
            win.Show();
        }

        // --------------------------
        // ListBox UX
        // --------------------------
        private void ListBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape && sender is ListBox lb) {
                lb.SelectedItem = null;
                e.Handled = true;
            }
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (sender is ListBoxItem item && !item.IsSelected) {
                item.IsSelected = true;
                item.Focus();
            }
        }

        // --------------------------
        // Search box
        // --------------------------
        private void txtSearch_GotFocus(object sender, RoutedEventArgs e) {
            if (txtSearch.Text == PlaceholderText) {
                txtSearch.Text = string.Empty;
                txtSearch.Foreground = InputBrush;
            }
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(txtSearch.Text)) {
                txtSearch.Text = PlaceholderText;
                txtSearch.Foreground = PlaceholderBrush;
            }
        }

        private void txtSearch_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape)
                txtSearch.Text = string.Empty;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
            if (!IsLoaded) return;

            var raw = txtSearch.Text;
            _searchText = string.IsNullOrWhiteSpace(raw) || raw == PlaceholderText ? "" : raw.Trim();

            txtSearch.BorderBrush = string.IsNullOrEmpty(_searchText)
                ? DefaultBorderBrush
                : (_vm.Vaults.Any(v => (v.AppName ?? "").IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    ? MatchBorderBrush
                    : NoResultBorderBrush);

            _vaultsView.Refresh();
        }

        // --------------------------
        // Filter menus
        // --------------------------
        private void cmdFilter_Click(object sender, RoutedEventArgs e) => OpenContextMenu(sender as Button);

        private void FilterMenu_IsPasswdSet_Click(object sender, RoutedEventArgs e) => SetSingleFilter("IsPasswdSet");
        private void FilterMenu_IsUserNameSet_Click(object sender, RoutedEventArgs e) => SetSingleFilter("IsUserNameSet");
        private void FilterMenu_IsAppNameSet_Click(object sender, RoutedEventArgs e) => SetSingleFilter("IsAppNameSet");
        private void FilterMenu_BitRate265_Click(object sender, RoutedEventArgs e) => SetSingleFilter("BitRate256");
        private void FilterMenu_BitRate192_Click(object sender, RoutedEventArgs e) => SetSingleFilter("BitRate192");
        private void FilterMenu_BitRate128OrLess_Click(object sender, RoutedEventArgs e) => SetSingleFilter("BitRate128");
        private void FilterMenu_StatusGood_Click(object sender, RoutedEventArgs e) => SetSingleFilter("StatusGood");
        private void FilterMenu_StatusBad_Click(object sender, RoutedEventArgs e) => SetSingleFilter("StatusBad");
        private void FilterMenu_RemoveFilter_Click(object sender, RoutedEventArgs e) => SetSingleFilter("RemoveFilter");
        private async void FilterMenu_Reset_Click(object sender, RoutedEventArgs e) => await ResetListsAsync();

        private enum FilterKind {
            IsPasswdSet,
            IsUserNameSet,
            IsAppNameSet,
            BitRate256,
            BitRate192,
            BitRate128,
            StatusGood,
            StatusBad
        }

        // --------------------------
        // Sort menus
        // --------------------------
        private void cmdSort_Click(object sender, RoutedEventArgs e) => OpenContextMenu(sender as Button);

        private void SortMenu_Alphabetical_Click(object sender, RoutedEventArgs e) => ApplySort("Alphabetical");
        private void SortMenu_DateCreated_Click(object sender, RoutedEventArgs e) => ApplySort("DateCreated");
        private void SortMenu_Bitrate_Click(object sender, RoutedEventArgs e) => ApplySort("Bitrate");
        private void SortMenu_Status_Click(object sender, RoutedEventArgs e) => ApplySort("Status");
        private void SortMenu_RemoveSort_Click(object sender, RoutedEventArgs e) => ApplySort("RemoveSort");
        private async void SortMenu_Reset_Click(object sender, RoutedEventArgs e) => await ResetListsAsync();

        private enum SortKind {
            Alphabetical,
            DateCreated,
            Bitrate,
            Status
        }

        private void ApplySort(string kind) {
            if (_vaultsView is null) return;

            _vaultsView.SortDescriptions.Clear();

            switch (kind) {
                case "Alphabetical":
                    _vaultsView.SortDescriptions.Add(new SortDescription(nameof(AppVault.AppName), ListSortDirection.Ascending));
                    break;
                case "DateCreated":
                    _vaultsView.SortDescriptions.Add(new SortDescription(nameof(AppVault.DateCreated), ListSortDirection.Ascending));
                    break;
                case "Bitrate":
                    _vaultsView.SortDescriptions.Add(new SortDescription(nameof(AppVault.BitRate), ListSortDirection.Ascending));
                    break;
                case "Status":
                    _vaultsView.SortDescriptions.Add(new SortDescription(nameof(AppVault.IsStatusGood), ListSortDirection.Ascending));
                    break;
                case "RemoveSort":
                    break;
            }
        }


        private void SetSingleFilter(string filterKey) {
            _filters.Clear();
            if (filterKey != "RemoveFilter")
                _filters.Add(filterKey);

            _vaultsView.Refresh();
        }

        // --------------------------
        // Reset (refresh from DB + keep search text refresh behavior)
        // --------------------------
        private async Task ResetListsAsync() {
            txtSearch.BorderBrush = DefaultBorderBrush;

            await _vm.RefreshVaultsAsync();
            _vm.UpdateAllVaultStatus();
            await _vm.ComposeStatusMSG();

            _preSortSnapshot = null;
            _preFilterSnapshot = null;
            _sortedActive = false;
            _filteredActive = false;

            // Keep search behavior: if user had text, force re-run TextChanged
            if (!string.IsNullOrWhiteSpace(txtSearch.Text) && txtSearch.Text != PlaceholderText) {
                var txt = txtSearch.Text;
                txtSearch.Text = string.Empty;
                txtSearch.Text = txt;
                return;
            }

            _filters.Clear();
            _searchText = "";

            txtSearch.Text = PlaceholderText;
            txtSearch.Foreground = PlaceholderBrush;
            txtSearch.BorderBrush = DefaultBorderBrush;

            _vaultsView.SortDescriptions.Clear();
            _vaultsView.Refresh();
        }

        private static void OpenContextMenu(Button? btn) {
            if (btn?.ContextMenu is null) return;

            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            btn.ContextMenu.IsOpen = true;
        }

        private void FilterMenu_Cancel_Click(object sender, RoutedEventArgs e) {
            // NOTHING HERE, IT WILL JUST CLOSE.
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

        }

        private void SortMenu_Cancel_Click(object sender, RoutedEventArgs e) {
            // Intentionally empty - closing the menu is enough.
        }

        public void OpenSettingsWindow() {
            var win = new SettingsWindow(_vm.RefreshVaultsAsync);
            win.Show();
        }

        private bool VaultFilter(object obj) {
            if (obj is not AppVault v) return false;

            // Search
            if (!string.IsNullOrWhiteSpace(_searchText)) {
                if (string.IsNullOrWhiteSpace(v.AppName)) return false;
                if (v.AppName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) < 0) return false;
            }

            // Filters
            foreach (var f in _filters) {
                switch (f) {
                    case "IsPasswdSet": if (!v.IsPasswdSet) return false; break;
                    case "IsUserNameSet": if (!v.IsUserNameSet) return false; break;
                    case "IsAppNameSet": if (!v.IsAppNameSet) return false; break;
                    case "BitRate256": if (v.BitRate < 192 || v.BitRate > 256) return false; break;
                    case "BitRate192": if (v.BitRate < 128 || v.BitRate > 192) return false; break;
                    case "BitRate128": if (v.BitRate < 8 || v.BitRate > 128) return false; break;
                    case "StatusGood": if (!v.IsStatusGood) return false; break;
                    case "StatusBad": if (v.IsStatusGood) return false; break;
                }
            }

            return true;
        }

        private void RestartApp_Click(object sender, RoutedEventArgs e) {
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }
    }
}