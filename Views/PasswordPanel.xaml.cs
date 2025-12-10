using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.ViewModels;
using System.Windows;
using System.Windows.Controls;

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
                new Helper("First, select a vault to edit.").Show();
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
                new Helper("First, select a vault to edit.").Show();
                return;
            }

            mainVM.DeleteVaultEntryCommand.Execute(vault);
        }
    }
}
