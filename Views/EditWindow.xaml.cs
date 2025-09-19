using Passwd_VaultManager.Models;
using Passwd_VaultManager.ViewModels;
using System.Windows;

namespace Passwd_VaultManager.Views
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        private readonly EditWindowVM _vm = new();

        public EditWindow(AppVault g)
        {
            InitializeComponent();
            DataContext = _vm;

            //_vm.AppName = g.AppName;
        }


    }
}
