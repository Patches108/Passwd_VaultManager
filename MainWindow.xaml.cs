using Passwd_VaultManager.Models;
using Passwd_VaultManager.ViewModels;
using Passwd_VaultManager.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace Passwd_VaultManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly PasswdPanelVM _vm = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;

            _vm.testLoad();

            ObservableCollection<AppVault> testList = _vm.GetAppVaults();

            foreach (var v in testList) {
                itms_PasswdPanelList.Items.Add(v);
            }
        }

        private void frmMainWindow_Loaded(object sender, RoutedEventArgs e) {

        }

        private void cmdClose_Click(object sender, RoutedEventArgs e) {
            // TODO
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

        }

        private void NewVaultRecord_Click(object sender, RoutedEventArgs e) {

        }

        private void EditVaultRecord_Click(object sender, RoutedEventArgs e) {

        }

        private void DeleteVaultRecord_Click(object sender, RoutedEventArgs e) {

        }

        private void ExitApp_Click(object sender, RoutedEventArgs e) {

        }
    }
}