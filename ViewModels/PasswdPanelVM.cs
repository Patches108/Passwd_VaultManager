using Passwd_VaultManager.Models;
using System.Windows.Input;

namespace Passwd_VaultManager.ViewModels {


    /// <summary>
    /// ViewModel for the password panel.
    /// 
    /// Manages selection state for a vault entry and exposes commands
    /// used by the password display/selection UI.
    /// </summary>
    public sealed class PasswdPanelVM : ViewModelBase {

        private AppVault? _selectedAppVault;
        public ICommand DeleteSelectedCommand { get; }
        public ICommand SelectCommand { get; }



        /// <summary>
        /// Initializes commands used by the password panel.
        /// </summary>
        public PasswdPanelVM() {

            SelectCommand = new RelayCommand(
                p => SelectVault(p as AppVault),
                p => p is AppVault);
        }



        /// <summary>
        /// Gets or sets the currently selected vault entry.
        /// </summary>
        public AppVault? SelectedAppVault {
            get => _selectedAppVault;
            set {
                if (_selectedAppVault == value)
                    return;

                _selectedAppVault = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AppName));
            }
        }



        /// <summary>
        /// Gets the application/account name of the selected vault entry.
        /// </summary>
        public string AppName => SelectedAppVault?.AppName ?? string.Empty;




        /// <summary>
        /// Sets the specified vault entry as the current selection.
        /// </summary>
        /// <param name="vault">The vault entry to select.</param>
        private void SelectVault(AppVault? vault) {
            if (vault == null)
                return;

            SelectedAppVault = vault;
        }
    }
}
