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

        private void cmdCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void cmdManuallyEnterPasswd_Click(object sender, RoutedEventArgs e) {

        }

        private void cmdGenPasswd_Click(object sender, RoutedEventArgs e) {
            var gen = new Passwd_VaultManager.Funcs.PasswdGen();
            string pw = gen.GenPassword(bitRate: 128, len: 16); // returns at least 21 chars here
            txtPasswd.Text = pw;
        }
    }
}
