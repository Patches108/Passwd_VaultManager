using Passwd_VaultManager.Models;
using System.Collections.ObjectModel;
using System.Windows;
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
        public ICommand SelectCommand {  get; }


        public PasswdPanelVM() {
            EditSelectedCommand = new RelayCommand(_ => EditSelected(), _ => SelectedAppVault != null);
            DeleteSelectedCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedAppVault != null);
            SelectCommand = new RelayCommand(param => OnSelect(param as AppVault), param => param is AppVault);
        }

        private void OnSelect(AppVault appVault) {
            Guid id = appVault.getAppVaultInstanceGuid;
            SelectedAppVault = appVault;
        }

        private void EditSelected() {
            // open an editor dialog, or toggle an edit mode, etc.
        }

        private void DeleteSelected() {
            if (SelectedAppVault is null) {
                // APPROPIATE ERROR MESSAGE and ERROR HANDLING.
                return;
            }

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
    }
}
