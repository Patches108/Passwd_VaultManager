using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Passwd_VaultManager.Views {
    public partial class PasswordPanel : UserControl {

        MainWindowVM mainVM = (MainWindowVM)Application.Current.MainWindow.DataContext;

        public PasswordPanel() {
            InitializeComponent();
        }

        private void Edit_OnClick(object sender, System.Windows.RoutedEventArgs e) {
            
            if(mainVM is null) {
                new MessageWindow("ERROR: Failed to get view model instance.", SoundController.ErrorSound).ShowDialog();
                return; 
            }
            
            AppVault vault = mainVM.SelectedAppVault; // Get selected vault
            if (vault is null) {
                new Helper("First, select a vault to edit.", SoundController.ErrorSound).Show();
                return;
            }

            var vm = new EditWindowVM(vault);
            var win = new EditWindow { DataContext = vm };
            win.ShowDialog();
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e) {
            Delete_OnClick(sender, e, mainVM);
        }

        private void Delete_OnClick(object sender, System.Windows.RoutedEventArgs e, MainWindowVM mainVM) {
            if (mainVM is null) {
                new MessageWindow("ERROR: Failed to get view model instance.", SoundController.ErrorSound).ShowDialog();
                return;
            }

            AppVault vault = mainVM.SelectedAppVault; // Get selected vault
            if (vault is null) {
                new Helper("First, select a vault to edit.", SoundController.ErrorSound).Show();
                return;
            }

            mainVM.DeleteVaultEntryCommand.Execute(vault);
        }

        private void PP_Loaded(object sender, RoutedEventArgs e) {
            SharedFuncs.Apply(this, App.Settings);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (sender is FrameworkElement element && element.ContextMenu != null) {
                element.ContextMenu.PlacementTarget = element;
                element.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                element.ContextMenu.IsOpen = true;
            }
        }

        private void cmdCopyToClopboard(object sender, RoutedEventArgs e) {
            // 1) Get the vault for THIS panel (no prior click required)
            if (DataContext is not AppVault vault) {
                new MessageWindow("ERROR: Vault not found for this panel.", SoundController.ErrorSound).ShowDialog();
                return;
            }

            // 2) Optionally set it as the selected vault in the main VM (so Edit/Delete etc. stay consistent)
            var mainVM = Application.Current.MainWindow?.DataContext as MainWindowVM;
            if (mainVM != null)
                mainVM.SelectedAppVault = vault;

            // 3) Copy password
            if (!string.IsNullOrWhiteSpace(vault.Password)) {
                Clipboard.SetText(vault.Password);
                new ToastNotification("Text copied to clipboard", true, SoundController.SuccessSound).Show();
            } else {
                new MessageWindow("ERROR: Password field is empty.", SoundController.ErrorSound).ShowDialog();
            }
        }

    }
}
