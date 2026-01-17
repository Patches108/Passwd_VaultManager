using Microsoft.Win32;
using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Passwd_VaultManager.ViewModels {
    internal sealed class MainWindowVM : ViewModelBase {
        private const string StartupAppName = "PasswordVaultManager";
        private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        private AppVault? _selectedAppVault;
        private bool _isStartupEnabled;
        private string _statusMsg = string.Empty;

        public ObservableCollection<AppVault> Vaults { get; } = new();

        // If you actually use these, make them snapshots; otherwise delete them.
        public ObservableCollection<AppVault> PreSortedVaults { get; private set; } = new();
        public ObservableCollection<AppVault> PreFilteredVaults { get; private set; } = new();

        // If this is required, initialize it; otherwise remove.
        public PasswdPanelVM PasswdPanelVM { get; } = new();

        public ICommand EditVaultEntryCommand { get; }
        public ICommand NewVaultEntryCommand { get; }
        public ICommand DeleteVaultEntryCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenAboutCommand { get; }
        public ICommand RefreshCommand { get; }

        public MainWindowVM() {
            EditVaultEntryCommand = new RelayCommand(
                p => OpenEditWindow(p as AppVault ?? SelectedAppVault),
                p => (p as AppVault ?? SelectedAppVault) is not null);

            DeleteVaultEntryCommand = new RelayCommand(
                async p => await DeleteVaultEntryAsync(p as AppVault ?? SelectedAppVault),
                p => (p as AppVault ?? SelectedAppVault) is not null);


            NewVaultEntryCommand = new RelayCommand(_ => OpenNewVaultEntry());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
            OpenAboutCommand = new RelayCommand(_ => OpenAboutWin());
            RefreshCommand = new RelayCommand(async _ => await RefreshVaultsAsync());

            // Load startup setting
            IsStartupEnabled = IsAppSetToRunOnStartup();
        }

        public AppVault? SelectedAppVault {
            get => _selectedAppVault;
            set {
                if (ReferenceEquals(_selectedAppVault, value)) return;
                _selectedAppVault = value;
                OnPropertyChanged();

                RaiseSelectionCommandsCanExecute();
            }
        }

        public bool IsStartupEnabled {
            get => _isStartupEnabled;
            set {
                if (_isStartupEnabled == value) return;
                _isStartupEnabled = value;
                OnPropertyChanged();

                // if registry write fails, revert and show error
                if (!TryRegisterInStartup(value, out var err)) {
                    _isStartupEnabled = !value;
                    OnPropertyChanged(nameof(IsStartupEnabled));

                    new MessageWindow($"Failed to update startup setting.\n\n{err}", SoundController.ErrorSound)
                        .ShowDialog();
                }
            }
        }

        public string StatusMSG {
            get => _statusMsg;
            set {
                if (_statusMsg == value) return;
                _statusMsg = value;
                OnPropertyChanged();
            }
        }

        private void RaiseSelectionCommandsCanExecute() {
            (DeleteVaultEntryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (EditVaultEntryCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        // -----------------------
        // Data refresh + status
        // -----------------------
        public async Task RefreshVaultsAsync() {
            var list = await DatabaseHandler.GetVaults();

            Vaults.Clear();
            foreach (var v in list) {
                NormalizeVaultStatus(v);
                Vaults.Add(v);
            }

            // Optional snapshots for later undo/filter features
            PreSortedVaults = new ObservableCollection<AppVault>(Vaults);
            PreFilteredVaults = new ObservableCollection<AppVault>(Vaults);
            OnPropertyChanged(nameof(PreSortedVaults));
            OnPropertyChanged(nameof(PreFilteredVaults));

            await ComposeStatusMSG();
        }

        private static void NormalizeVaultStatus(AppVault v) {
            v.IsUserNameSet = !string.IsNullOrWhiteSpace(v.UserName);
            v.IsPasswdSet = !string.IsNullOrWhiteSpace(v.Password);

            // Treat "No App/Account Name" as not-set
            bool hasRealName =
                !string.IsNullOrWhiteSpace(v.AppName) &&
                !v.AppName.Equals(AppVault.DefaultNoName, StringComparison.Ordinal);

            v.IsAppNameSet = hasRealName;

            v.IsStatusGood = v.IsUserNameSet && v.IsPasswdSet && v.IsAppNameSet;

            if (!hasRealName)
                v.SetNoName();
        }

        public async Task ComposeStatusMSG() {
            int recCount = await DatabaseHandler.GetRecordCountAsync();
            StatusMSG = $"{recCount} Vaults.";
        }

        public void UpdateAllVaultStatus() {
            foreach (var v in Vaults)
                NormalizeVaultStatus(v);
        }

        // -----------------------
        // Window actions
        // -----------------------
        private void OpenEditWindow(AppVault? vault) {
            if (vault is null) {
                new Helper("Oops!\n\nFirst, select a vault to edit.", SoundController.ErrorSound).Show();
                return;
            }

            var vm = new EditWindowVM(vault);
            var win = new EditWindow { DataContext = vm };
            win.ShowDialog();

            // After edit, refresh to reflect DB changes (if you want)
            // _ = RefreshVaultsAsync();
        }

        private void OpenNewVaultEntry() {
            // Pass an action rather than holding a Func<Task> field in the VM
            var win = new NewWindow(RefreshVaultsAsync);
            win.Show();
        }

        private async Task DeleteVaultEntryAsync(AppVault? vault) {
            if (vault is null) {
                new MessageWindow("ERROR: No Vault selected.", SoundController.ErrorSound).ShowDialog();
                return;
            }

            var confirm = new YesNoWindow(
                $"Are you sure you want to delete this record ({vault.AppName})? This cannot be undone.");

            bool confirmed = confirm.ShowDialog() == true && confirm.YesNoWin_Result;
            if (!confirmed) return;

            string vaultName = vault.AppName;

            try {
                Vaults.Remove(vault);
                await DatabaseHandler.DeleteVaultAsync(vault);
                await RefreshVaultsAsync();

                Application.Current.Dispatcher.Invoke(() => {
                    new ToastNotification(
                        $"Vault entry - ({vaultName}) - has been successfully deleted.",
                        true,
                        SoundController.SuccessSound).Show();
                });
            } catch (Exception ex) {
                new MessageWindow(
                    $"Failed to delete vault entry - ({vaultName}).\n\n{ex.Message}",
                    SoundController.ErrorSound).ShowDialog();

                // Recover UI list if needed
                await RefreshVaultsAsync();
            }
        }

        private void OpenSettings() {
            var win = new SettingsWindow(RefreshVaultsAsync);
            win.Show();
        }

        private void OpenAboutWin() {
            new About().Show();
        }

        // -----------------------
        // Startup registry
        // -----------------------
        public static bool IsAppSetToRunOnStartup() {
            try {
                using var rk = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
                var value = rk?.GetValue(StartupAppName) as string;
                if (string.IsNullOrWhiteSpace(value)) return false;

                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrWhiteSpace(exePath)) return false;

                return value.Contains(exePath, StringComparison.OrdinalIgnoreCase);
            } catch {
                return false;
            }
        }

        public static bool TryRegisterInStartup(bool enable, out string error) {
            error = string.Empty;

            try {
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrWhiteSpace(exePath)) {
                    error = "Could not determine executable path.";
                    return false;
                }

                using var rk = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
                if (rk is null) {
                    error = "Could not open registry Run key.";
                    return false;
                }

                if (enable) {
                    rk.SetValue(StartupAppName, $"\"{exePath}\"");
                } else {
                    rk.DeleteValue(StartupAppName, throwOnMissingValue: false);
                }

                return true;
            } catch (Exception ex) {
                error = ex.Message;
                return false;
            }
        }
    }
}
