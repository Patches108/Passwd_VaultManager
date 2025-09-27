using Passwd_VaultManager.ViewModels;
using System.Diagnostics;
using System.Windows;
using static Passwd_VaultManager.Funcs.PasswdGen;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for NewWindow.xaml
    /// </summary>
    public partial class NewWindow : Window {

        private readonly NewWindowVM _vm = new();
        
        private int _bitRate = 256;
        private int _len = 41;

        private string _passwdWhole = String.Empty;

        public NewWindow() {
            InitializeComponent();
            DataContext = _vm;
            lblPasswdStatus.Visibility = Visibility.Hidden;
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void cmdManuallyEnterPasswd_Click(object sender, RoutedEventArgs e) {
            sldPasswdLength.IsEnabled = false;
            lblPasswdStatus.Visibility = Visibility.Hidden;
            txtPasswd.Text = String.Empty;
            txtPasswd.Focus();
        }

        private void cmdGenPasswd_Click(object sender, RoutedEventArgs e) {
            sldPasswdLength.IsEnabled = true;
            var gen = new Passwd_VaultManager.Funcs.PasswdGen();
            string pw = gen.GenPassword(bitRate: _bitRate); // returns at least 21 chars here
            txtPasswd.Text = pw;

            _vm.BitRate = _bitRate;

            switch(_bitRate) {
                case 128:
                    _len = 21;
                    break;
                case 192:
                    _len = 31;
                    break;
                case 256:
                    _len = 41;
                    break;
            }

            _vm.Length = _len;
            
            _passwdWhole = txtPasswd.Text.Trim();

            lblPasswdStatus.Visibility = Visibility.Visible;
        }

        private void cmdCreateVault_Click(object sender, RoutedEventArgs e) {
            // 1. Checks for and disallows empty controls.
            if(String.IsNullOrWhiteSpace(txtAppName.Text) || String.IsNullOrWhiteSpace(txtPasswd.Text) || String.IsNullOrWhiteSpace(txtUserName.Text)) {
                // Display error screen
                return;
            }

            // 2. Creates vault.
            // 3. Saves it to DB.
        }

        private void rad_128_Click(object sender, RoutedEventArgs e) {
            _bitRate = 128;
            sldPasswdLength.Value = (double)21;
            sldPasswdLength.IsEnabled = false;
        }

        private void rad_256_Click(object sender, RoutedEventArgs e) {
            _bitRate = 256;
            sldPasswdLength.IsEnabled = true;
            sldPasswdLength.Value = (double)41;
        }

        private void txtAppName_GotFocus(object sender, RoutedEventArgs e) {
            if(String.IsNullOrWhiteSpace(txtAppName.Text) || txtAppName.Text.Equals("Website/Application Name"))
                txtAppName.Text = String.Empty;
        }

        private void txtUserName_GotFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtUserName.Text) || txtUserName.Text.Equals("User Name / Email"))
                txtUserName.Text = String.Empty;
        }

        private void txtPasswd_GotFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtPasswd.Text) || txtPasswd.Text.Equals("Passwd"))
                txtPasswd.Text = String.Empty;
        }

        private void txtAppName_LostFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtAppName.Text))
                txtAppName.Text = "Website/Application Name";
        }

        private void txtUserName_LostFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtUserName.Text))
                txtUserName.Text = "User Name / Email";
        }

        private void txtPasswd_LostFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtPasswd.Text))
                txtPasswd.Text = "Passwd";
        }

        private void rad_192_Click(object sender, RoutedEventArgs e) {
            _bitRate = 192;
            sldPasswdLength.IsEnabled = true;
            sldPasswdLength.Value = (double)31;
        }

        private void cmdCopyToClopboard(object sender, RoutedEventArgs e) {
            // Copy password text field data to clipboard
            if (!String.IsNullOrWhiteSpace(txtPasswd.Text.Trim()))
                Clipboard.SetText(txtPasswd.Text);
            else { 
                // ERROR MESSAGE in message window.
            }
        }

        private void RunCleanup(object sender, System.ComponentModel.CancelEventArgs e) {
            txtPasswd.Text = String.Empty;
            txtUserName.Text = String.Empty;
            txtAppName.Text = String.Empty;

            Clipboard.SetText("");

            // Run GC
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void sldPasswdLength_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {

            if (String.IsNullOrWhiteSpace(txtPasswd.Text)) return;

            int roundedValue = (int)sldPasswdLength.Value;
            _vm.SliderValue = roundedValue.ToString();

            //txtPasswd.Text = _passwdWhole[..roundedValue];
            if(_passwdWhole.Length > 0 && roundedValue <= _passwdWhole.Length)
                txtPasswd.Text = _passwdWhole[..roundedValue];
        }

        private void txtCharactersToExclude_GotFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtCharactersToExclude.Text) || txtCharactersToExclude.Text.Equals("Chars to Exclude"))
                txtCharactersToExclude.Text = String.Empty;
        }

        private void txtCharactersToExclude_LostFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtCharactersToExclude.Text))
                txtCharactersToExclude.Text = "Chars to Exclude";
        }
    }
}
