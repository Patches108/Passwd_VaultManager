using Microsoft.Win32;
using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Passwd_VaultManager.ViewModels
{
    internal sealed class MainWindowVM : ViewModelBase {

        private readonly Func<Task> _refreshAction;

        private AppVault? _selectedAppVault;

        private bool _isStartupEnabled;

        public ICommand EditVaultEntryCommand { get; }
        public ICommand NewVaultEntryCommand { get; }
        public ICommand DeleteVaultEntryCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenAboutCommand { get; }

        public ObservableCollection<AppVault> Vaults { get; } = new();

        public MainWindowVM() {

            _refreshAction = RefreshVaultsAsync;

            EditVaultEntryCommand = new RelayCommand(param => OpenEditWindow((AppVault)param));
            NewVaultEntryCommand = new RelayCommand(_ => OpenNewVaultEntry(_refreshAction));
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
            OpenAboutCommand = new RelayCommand(_ => OpenABoutWin());

            DeleteVaultEntryCommand = new RelayCommand(
                p => DeleteVaultEntry(p as AppVault),
                p => p is AppVault);

            _isStartupEnabled = IsAppSetToRunOnStartup();
            IsStartupEnabled = IsAppSetToRunOnStartup();
        }

        public bool IsStartupEnabled {
            get => _isStartupEnabled;
            set {
                if (_isStartupEnabled != value) {
                    _isStartupEnabled = value;
                    OnPropertyChanged();
                    RegisterInStartup(value); // call your registry method
                }
            }
        }

        public AppVault? SelectedAppVault {
            get => _selectedAppVault;
            set {
                if (_selectedAppVault == value)
                    return;

                _selectedAppVault = value;
                OnPropertyChanged();

                // Notify delete button
                (DeleteVaultEntryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public PasswdPanelVM PasswdPanelVM { get; set; }

        public async Task RefreshVaultsAsync() {
            Vaults.Clear();
            var list = await DatabaseHandler.GetVaults();
            foreach (var v in list) Vaults.Add(v);
        }

        private void OpenEditWindow(AppVault vault) {

            if (vault is null) {
                new Helper("Opps!\n\nFirst, select a vault to edit.").Show();
                return;
            }

            var vm = new EditWindowVM(vault);
            var win = new EditWindow { DataContext = vm };
            win.ShowDialog();
        }

        private void OpenNewVaultEntry(Func<Task> refresh) {
            var win = new NewWindow(refresh);
            win.Show();
        }

        private async void DeleteVaultEntry(AppVault vault) {
            if(vault is null) {
                new MessageWindow("ERROR: No Vault unit detected").ShowDialog();
            } else {
                
                // confirm with yes/no dialog
                YesNoWindow confirm = new YesNoWindow($"Are you sure you want to delete this record ({vault.AppName})? This cannot be undone.");
                bool confirmed = confirm.ShowDialog() == true && confirm.YesNoWin_Result;

                if (confirmed) {
                    
                    string s_VaultName = vault.AppName;

                    Vaults.Remove(vault);
                    await DatabaseHandler.DeleteVaultAsync(vault);
                    await RefreshVaultsAsync();

                    System.Windows.Application.Current.Dispatcher.Invoke(() => {
                        var toast = new ToastNotification($"Vault entry - ({s_VaultName}) - has been successfully deleted.", true);
                        toast.Show();
                    });
                }                
            }
        }

        private void OpenSettings() {
            var win = new SettingsWindow();
            win.Show();
        }

        private void OpenABoutWin() {
            var win = new About();
            win.Show();
        }


        public static bool IsAppSetToRunOnStartup() {
            string appName = "PasswordVaultManager";
            using RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);

            var value = rk?.GetValue(appName) as string;
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            return value != null && value.Contains(exePath, StringComparison.OrdinalIgnoreCase);
        }

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
