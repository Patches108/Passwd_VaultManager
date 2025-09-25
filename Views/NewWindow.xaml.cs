using Passwd_VaultManager.ViewModels;
using System.Diagnostics;
using System.Windows;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for NewWindow.xaml
    /// </summary>
    public partial class NewWindow : Window {

        private readonly NewWindowVM _vm = new();
        private int _bitRate = 128;

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

        private void cmdCreateVault_Click(object sender, RoutedEventArgs e) {
            // 1. Checks for and disallows empty controls.
            // 2. Creates vault.
            // 3. Saves it to DB.
        }

        private void rad_128_Click(object sender, RoutedEventArgs e) {
            _bitRate = 128;
            Debug.WriteLine($"_bitRate: {_bitRate}");
        }

        private void rad_256_Click(object sender, RoutedEventArgs e) {
            _bitRate = 256;
            Debug.WriteLine($"_bitRate: {_bitRate}");
        }

        private void txtAppName_GotFocus(object sender, RoutedEventArgs e) {
            if(String.IsNullOrEmpty(txtAppName.Text) || txtAppName.Text.Equals("Website/Application Name"))
                txtAppName.Text = String.Empty;
        }

        private void txtUserName_GotFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrEmpty(txtUserName.Text) || txtUserName.Text.Equals("User Name"))
                txtUserName.Text = String.Empty;
        }

        private void txtPasswd_GotFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrEmpty(txtPasswd.Text) || txtPasswd.Text.Equals("Passwd"))
                txtPasswd.Text = String.Empty;
        }

        private void txtAppName_LostFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrEmpty(txtAppName.Text))
                txtAppName.Text = "Website/Application Name";
        }

        private void txtUserName_LostFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrEmpty(txtUserName.Text))
                txtUserName.Text = "User Name";
        }

        private void txtPasswd_LostFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrEmpty(txtPasswd.Text))
                txtPasswd.Text = "Passwd";
        }
    }
}
