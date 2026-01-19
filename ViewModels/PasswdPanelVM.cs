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
