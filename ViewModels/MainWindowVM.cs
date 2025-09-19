using Passwd_VaultManager.Models;
using Passwd_VaultManager.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Passwd_VaultManager.ViewModels
{
    internal class MainWindowVM : ViewModelBase
    {

        public ICommand EditVaultEntryCommand { get; }
        public ICommand NewVaultEntryCommand { get; }
        public ICommand DeleteVaultEntryCommand { get; }

        //private PasswdPanelVM passwdVM = new PasswdPanelVM();

        public MainWindowVM() {
            EditVaultEntryCommand = new RelayCommand(param => OpenEditWindow((AppVault)param));
            NewVaultEntryCommand = new RelayCommand(_ => OpenNewVaultEntry());


            //ObservableCollection<AppVault> testList = PasswdPanelVM.GetAppVaults();
        }

        private void OpenEditWindow(AppVault vault) {
            if (vault is null) return;

            EditWindow editWin = new EditWindow(vault);
            editWin.Show();
        }

        private void OpenNewVaultEntry() {
            NewWindow win = new NewWindow();
            win.Show();
        }

    }
}
