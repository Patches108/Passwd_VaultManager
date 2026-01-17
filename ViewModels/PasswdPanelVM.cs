using Passwd_VaultManager.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Passwd_VaultManager.ViewModels {
    public sealed class PasswdPanelVM : ViewModelBase {

        private AppVault? _selectedAppVault;
        public ICommand DeleteSelectedCommand { get; }
        public ICommand SelectCommand { get; }

        public PasswdPanelVM() {

            SelectCommand = new RelayCommand(
                p => SelectVault(p as AppVault),
                p => p is AppVault);
        }

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

        public string AppName => SelectedAppVault?.AppName ?? string.Empty;

        private void SelectVault(AppVault? vault) {
            if (vault == null)
                return;

            SelectedAppVault = vault;
        }
    }
}
