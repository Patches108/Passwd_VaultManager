using Passwd_VaultManager.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Passwd_VaultManager.ViewModels
{
    internal class PasswdPanelVM : ViewModelBase
    {
        public ObservableCollection<AppVault> AppVaults { get; } = new();

        private Guid _tempGuid;

        // ICommands
        public ICommand EditSelectedCommand { get; }
        public ICommand DeleteSelectedCommand { get; }

        public PasswdPanelVM() {
            EditSelectedCommand = new RelayCommand(_ => EditSelected(), _ => SelectedAppVault != null);
            DeleteSelectedCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedAppVault != null);
        }

        private void EditSelected() {
            // open an editor dialog, or toggle an edit mode, etc.
        }

        private void DeleteSelected() {
            if (SelectedAppVault is null) return;
            AppVaults.Remove(SelectedAppVault);
            SelectedAppVault = AppVaults.FirstOrDefault();
        }

        private AppVault _selectedAppVault;

        public AppVault SelectedAppVault {
            get => _selectedAppVault;
            set {
                if(_selectedAppVault != value) {
                    _selectedAppVault = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AppVault));
                }
            }
        }

        public ObservableCollection<AppVault> GetAppVaults() {
            return AppVaults;
        }

        public void temporarilyStoreGuid(Guid g) {
            _tempGuid = g;
        }

        public string AppName => SelectedAppVault?.AppName ?? string.Empty;

        public void testLoad() {
            //AppVaults.Add(new AppVault { AppName = "Gmail", UserName = "TestUsername1", Password = "1234", IsPasswdSet = true, IsUserNameSet = true, IsStatusGood=true });
            //AppVaults.Add(new AppVault { AppName = "someMail", UserName = "TestUsername2", Password = "1234", IsPasswdSet = true, IsUserNameSet = false, IsStatusGood = false });
            //AppVaults.Add(new AppVault { AppName = "WorkMail", UserName = "TestUsername3", Password = "1234", IsPasswdSet = false, IsUserNameSet = true, IsStatusGood = true });

            //SelectedAppVault = AppVaults[0];
        }
    }
}
