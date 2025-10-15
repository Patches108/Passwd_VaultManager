using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
//using static Passwd_VaultManager.MainWindow;

namespace Passwd_VaultManager.ViewModels
{
    internal sealed class MainWindowVM : ViewModelBase {

        private readonly Func<Task> _refreshAction;

        private AppVault? _selectedAppVault;

        public ICommand EditVaultEntryCommand { get; }
        public ICommand NewVaultEntryCommand { get; }
        public ICommand DeleteVaultEntryCommand { get; }

        public ObservableCollection<AppVault> Vaults { get; } = new();

        public MainWindowVM() {

            _refreshAction = RefreshVaultsAsync;

            EditVaultEntryCommand = new RelayCommand(param => OpenEditWindow((AppVault)param));
            NewVaultEntryCommand = new RelayCommand(_ => OpenNewVaultEntry(_refreshAction));

            DeleteVaultEntryCommand = new RelayCommand(
                p => DeleteVaultEntry(p as AppVault),
                p => p is AppVault);
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
            if (vault is null) return;

            var vm = new EditWindowVM(vault);
            var win = new EditWindow { DataContext = vm };
            win.ShowDialog();
        }

        private void OpenNewVaultEntry(Func<Task> refresh) {
            var win = new NewWindow(refresh);
            win.Show();
        }

        //public void TriggerLoadVaults() {
        //    //Load vaults on main form.
        //}

        private async void DeleteVaultEntry(AppVault vault) {
            if(vault is null) {
                new MessageWindow("ERROR: No Vault unit detected").ShowDialog();
            } else {
                
                // confirm with yes/no dialog
                YesNoWindow confirm = new YesNoWindow($"Are you sure you want to delete this record ({vault.AppName})? This cannot be undone.");
                bool confirmed = confirm.ShowDialog() == true && confirm.YesNoWin_Result;

                if (confirmed) {
                    // DB DELETE
                    Vaults.Remove(vault);
                    await DatabaseHandler.DeleteVaultAsync(vault);
                    await RefreshVaultsAsync();
                }                
            }
        }

    }
}
