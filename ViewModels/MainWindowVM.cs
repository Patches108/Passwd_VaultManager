// Password Vault Manager
// Copyright © 2026 Max C (aka Finn).
// All rights reserved.
//
// Licensed under the Password Vault Manager Source-Available License.
// Non-commercial use only.
//
// You may view, use, and modify this source code for personal,
// non-commercial purposes. Redistribution (including modified
// versions and compiled binaries) is permitted only if no fee
// is charged and this copyright notice and license are included.
//
// Commercial use, sale of binaries, or distribution for profit
// requires explicit written permission from the copyright holder.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND.
// See the LICENSE file in the project root for full terms.


using Microsoft.Win32;
using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Passwd_VaultManager.ViewModels {

    /// <summary>
    /// Main window ViewModel.
    /// 
    /// Manages the vault list, selection-dependent commands (new/edit/delete),
    /// status text, and the “run on startup” option via the Windows registry.
    /// </summary>
    internal sealed class MainWindowVM : ViewModelBase {
        private const string StartupAppName = "PasswordVaultManager";
        private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        private AppVault? _selectedAppVault;
        private bool _isStartupEnabled;
        private string _statusMsg = string.Empty;

        public ObservableCollection<AppVault> Vaults { get; } = new();

        public ObservableCollection<AppVault> PreSortedVaults { get; private set; } = new();
        public ObservableCollection<AppVault> PreFilteredVaults { get; private set; } = new();

        public PasswdPanelVM PasswdPanelVM { get; } = new();

        public ICommand EditVaultEntryCommand { get; }
        public ICommand NewVaultEntryCommand { get; }
        public ICommand DeleteVaultEntryCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenAboutCommand { get; }
        public ICommand RefreshCommand { get; }



        /// <summary>
        /// Initializes commands and loads the current “run on startup” setting.
        /// </summary>
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



        /// <summary>
        /// Gets or sets the currently selected vault entry in the UI.
        /// Updating this value refreshes command availability.
        /// </summary>
        public AppVault? SelectedAppVault {
            get => _selectedAppVault;
            set {
                if (ReferenceEquals(_selectedAppVault, value)) return;
                _selectedAppVault = value;
                OnPropertyChanged();

                RaiseSelectionCommandsCanExecute();
            }
        }



        /// <summary>
        /// Gets or sets whether the application should run at Windows startup.
        /// When changed, the corresponding registry value is updated.
        /// </summary>
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



        /// <summary>
        /// Gets or sets the UI status message displayed in the main window.
        /// </summary>
        public string StatusMSG {
            get => _statusMsg;
            set {
                if (_statusMsg == value) return;
                _statusMsg = value;
                OnPropertyChanged();
            }
        }



        /// <summary>
        /// Forces WPF to re-evaluate whether selection-dependent commands can execute.
        /// </summary>
        private void RaiseSelectionCommandsCanExecute() {
            (DeleteVaultEntryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (EditVaultEntryCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Reloads vault entries from the database, updates derived status flags,
        /// refreshes snapshot collections, and updates the status message.
        /// </summary>
        /// <returns>A task that completes when refresh is finished.</returns>
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



        /// <summary>
        /// Normalizes derived status flags for a vault entry (e.g., whether required
        /// fields are set) and applies the default “no name” behavior when needed.
        /// </summary>
        /// <param name="v">The vault entry to normalize.</param>
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



        /// <summary>
        /// Rebuilds the status message (e.g., record count) from the database.
        /// </summary>
        /// <returns>A task that completes when the message has been updated.</returns>
        public async Task ComposeStatusMSG() {
            int recCount = await DatabaseHandler.GetRecordCountAsync();
            StatusMSG = $"{recCount} Vaults.";
        }



        /// <summary>
        /// Recomputes derived status flags for all vault entries currently loaded.
        /// </summary>
        public void UpdateAllVaultStatus() {
            foreach (var v in Vaults)
                NormalizeVaultStatus(v);
        }




        /// <summary>
        /// Opens the edit window for the specified vault entry.
        /// If no entry is provided/selected, a helper message is shown.
        /// </summary>
        /// <param name="vault">The vault entry to edit.</param>
        private void OpenEditWindow(AppVault? vault) {
            if (vault is null) {
                new Helper("Oops!\n\nFirst, select a vault to edit.", SoundController.ErrorSound).Show();
                return;
            }

            var vm = new EditWindowVM(vault);
            var win = new EditWindow { DataContext = vm };
            win.ShowDialog();
        }



        /// <summary>
        /// Opens the window used to create a new vault entry.
        /// </summary>
        private void OpenNewVaultEntry() {
            // Pass an action rather than holding a Func<Task> field in the VM
            var win = new NewWindow(RefreshVaultsAsync);
            win.Show();
        }



        /// <summary>
        /// Deletes the specified vault entry after user confirmation,
        /// then refreshes the list to reflect database changes.
        /// </summary>
        /// <param name="vault">The vault entry to delete.</param>
        /// <returns>A task that completes when deletion and refresh are finished.</returns>
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



        /// <summary>
        /// Opens the settings window.
        /// </summary>
        private void OpenSettings() {
            var win = new SettingsWindow(RefreshVaultsAsync);
            win.Show();
        }



        /// <summary>
        /// Opens the About window.
        /// </summary>
        private void OpenAboutWin() {
            new About().Show();
        }



        /// <summary>
        /// Checks whether the application is currently registered to run on Windows startup.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a matching registry entry exists; otherwise <c>false</c>.
        /// </returns>
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



        /// <summary>
        /// Enables or disables running the application at Windows startup by updating
        /// the current user's Run registry key.
        /// </summary>
        /// <param name="enable">Whether to register (true) or unregister (false).</param>
        /// <param name="error">An error message when the operation fails.</param>
        /// <returns><c>true</c> on success; otherwise <c>false</c>.</returns>
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
