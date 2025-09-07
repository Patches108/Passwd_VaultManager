using Passwd_VaultManager.Models;
using System.Collections.ObjectModel;

namespace Passwd_VaultManager.ViewModels
{
    internal class PasswdPanelVM : ViewModelBase
    {
        public ObservableCollection<AppVault> AppVaults { get; } = new();

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

        public string AppName => SelectedAppVault?.AppName ?? string.Empty;

        public void testLoad() {
            AppVaults.Add(new AppVault { AppName = "Gmail", UserName = "TestUsername1", Password = "1234", IsPasswdSet = true, IsUserNameSet = true, IsStatusGood=true });
            AppVaults.Add(new AppVault { AppName = "someMail", UserName = "TestUsername2", Password = "1234", IsPasswdSet = true, IsUserNameSet = false, IsStatusGood = false });
            AppVaults.Add(new AppVault { AppName = "WorkMail", UserName = "TestUsername3", Password = "1234", IsPasswdSet = false, IsUserNameSet = true, IsStatusGood = true });

            SelectedAppVault = AppVaults[0];
        }
    }
}
