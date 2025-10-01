using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using static Passwd_VaultManager.MainWindow;

namespace Passwd_VaultManager.ViewModels
{

    //public interface IMainRefreshVaults {
    //    void RefreshVaults();
    //}

    internal class MainWindowVM : ViewModelBase {

        private readonly Func<Task> _refreshAction;

        public ICommand EditVaultEntryCommand { get; }
        public ICommand NewVaultEntryCommand { get; }
        public ICommand DeleteVaultEntryCommand { get; }

        public ObservableCollection<AppVault> Vaults { get; } = new();

        public MainWindowVM() {

            _refreshAction = RefreshVaultsAsync;

            EditVaultEntryCommand = new RelayCommand(param => OpenEditWindow((AppVault)param));
            NewVaultEntryCommand = new RelayCommand(_ => OpenNewVaultEntry(_refreshAction));
        }

        public async Task RefreshVaultsAsync() {
            Vaults.Clear();
            var list = await DatabaseHandler.GetVaults();
            foreach (var v in list) Vaults.Add(v);
        }

        private void OpenEditWindow(AppVault vault) {
            if (vault is null) return;

            EditWindow editWin = new EditWindow(vault);
            editWin.Show();
        }

        private void OpenNewVaultEntry(Func<Task> refresh) {
            var win = new NewWindow(refresh);
            win.Show();
        }

        public void TriggerLoadVaults() {
            //Load vaults on main form.
        }

    }
}
