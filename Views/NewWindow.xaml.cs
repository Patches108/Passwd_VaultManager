using Passwd_VaultManager.ViewModels;
using System.Windows;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for NewWindow.xaml
    /// </summary>
    public partial class NewWindow : Window {

        private readonly NewWindowVM _vm = new();

        public NewWindow() {
            InitializeComponent();
            DataContext = _vm;
        }
    }
}
