using Passwd_VaultManager.Models;
using Passwd_VaultManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Passwd_VaultManager.Views
{
    /// <summary>
    /// Interaction logic for PasswordPanel.xaml
    /// </summary>
    public partial class PasswordPanel : UserControl
    {
        PasswdPanelVM vm = new PasswdPanelVM();

        public PasswordPanel()
        {
            InitializeComponent();

            vm.PanelBorderBrush = new BrushConverter().ConvertFrom("#27D644") as Brush;
            vm.PanelBorderThickness = new Thickness(3);
        }

        private void PasswdPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (DataContext is AppVault vault) {
                Guid id = vault.getAppVaultInstanceGuid;
                vm.temporarilyStoreGuid(id);
                //MessageBox.Show($"Clicked panel for {vault.AppName} with ID {id}");
                // We use this GUID to find and edit the coorresponding panel in the edit form.
            }
        }
    }
}
