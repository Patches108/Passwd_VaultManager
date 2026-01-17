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
            //if (txtAppName_PP.Text.Equals("No App/Account Name"))
            //    txtAppName_PP.Foreground = Brushes.IndianRed;

            SharedFuncs.Apply(this, App.Settings);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (sender is FrameworkElement element && element.ContextMenu != null) {
                element.ContextMenu.PlacementTarget = element;
                element.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                element.ContextMenu.IsOpen = true;
            }
        }

    }
}
