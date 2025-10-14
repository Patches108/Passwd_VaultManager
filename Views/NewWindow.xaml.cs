using Passwd_VaultManager.Models;
using Passwd_VaultManager.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Passwd_VaultManager.Funcs;
//using static Passwd_VaultManager.Funcs.PasswdGen;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for NewWindow.xaml
    /// </summary>
    public partial class NewWindow : Window {

        private readonly NewWindowVM _vm = new();
        
        private int _bitRate = 256;
        private int _len = 41;
        private int _targetLength = 0;        // current slider length (0 = no limit)

        private bool _updating;
        private bool _showPlain;   // false = masked, true = reveal

        private string _passwdWhole = String.Empty;     // the generated full password
        private string _excludedChars = String.Empty;   // current exclusions from textbox

        private Func<Task> _refreshAction;

        public NewWindow(Func<Task> _refreshAction) {
            InitializeComponent();
            DataContext = _vm;

            this._refreshAction = _refreshAction;

            lblPasswdStatus.Visibility = Visibility.Hidden;
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void cmdManuallyEnterPasswd_Click(object sender, RoutedEventArgs e) {
            sldPasswdLength.IsEnabled = false;

            txtPasswd.IsReadOnly = false;   

            lblPasswdStatus.Visibility = Visibility.Hidden;
            
            txtPasswd.Text = String.Empty;
            txtPasswd.Focus();
        }

        private void cmdGenPasswd_Click(object sender, RoutedEventArgs e) {
            _updating = true; // suppress ValueChanged/TextChanged during setup
            try {
                txtCharactersToExclude.IsEnabled = true;
                sldPasswdLength.IsEnabled = true;
                txtPasswd.IsReadOnly = true;

                var gen = new Passwd_VaultManager.Funcs.PasswdGen();
                string pw = gen.GenPassword(bitRate: _bitRate);

                // 1) Set source-of-truth FIRST
                _passwdWhole = pw;

                // 2) Choose default target length from bitrate
                switch (_bitRate) {
                    case 128: _len = 21; sldPasswdLength.IsEnabled = false; break;
                    case 192: _len = 31; sldPasswdLength.IsEnabled = true; break;
                    case 256: _len = 41; sldPasswdLength.IsEnabled = true; break;
                }

                // 3) Update VM and slider without triggering our pipeline
                _vm.BitRate = _bitRate;
                _vm.Length = _len;
                _targetLength = _len;
                sldPasswdLength.Value = _len; // suppressed by _updating
                _vm.SliderValue = _len.ToString();
            } finally {
                _updating = false; // re-enable handlers
            }

            // 4) Now compute display from the pipeline (exclusions + length)
            UpdateDisplayedPassword(force: true);

            lblPasswdStatus.Visibility = Visibility.Visible;
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
                new MessageWindow("ERROR: Password field is empty. Generate or manually").ShowDialog();
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
            if (_updating) return;

            _targetLength = (int)Math.Round(e.NewValue);
            _vm.SliderValue = _targetLength.ToString();
            _vm.Length = txtPasswd.Text.Trim().Length;

            // Update bitrate
            switch (_vm.Length) {
                case >= 41:
                    _vm.BitRate = 256;
                    break;

                case >= 31 and < 41:
                    _vm.BitRate = 192;
                    break;

                case >= 21 and < 31:
                    _vm.BitRate = 128;
                    break;
            }

            UpdateDisplayedPassword();
        }

        private void UpdateDisplayedPassword(bool force = false) {
            if (_updating) return;

            _updating = true;
            try {
                txtPasswd.Text = SharedFuncs.BuildDisplay(
                    fullPassword: _passwdWhole.AsSpan(),
                    excludedChars: _excludedChars.AsSpan(),
                    targetLength: _targetLength,
                    currentText: txtPasswd.Text.AsSpan(),
                    force: false,
                    placeholder: "Passwd".AsSpan(),
                    mask: !_showPlain);
            } finally {
                _updating = false;
            }
        }

        private void txtCharactersToExclude_GotFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtCharactersToExclude.Text) || txtCharactersToExclude.Text.Equals("Chars to Exclude"))
                txtCharactersToExclude.Text = String.Empty;
        }

        private void txtCharactersToExclude_LostFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtCharactersToExclude.Text))
                txtCharactersToExclude.Text = "Chars to Exclude";
        }

        private void txtCharactersToExclude_TextChanged(object sender, TextChangedEventArgs e) {
            if (_updating) return;

            _excludedChars = txtCharactersToExclude.Text ?? string.Empty;

            // Ignore placeholder text
            if (_excludedChars.Trim().Equals("Chars to Exclude", StringComparison.Ordinal))
                _excludedChars = string.Empty;

            UpdateDisplayedPassword();
        }

        private void txtPasswd_TextChanged(object sender, TextChangedEventArgs e) {

            if (txtPasswd.Text.Trim().Length > 0)
                txtCharactersToExclude?.IsEnabled = true;

            if (txtPasswd.Text.Trim().Length >= 16)
                sldPasswdLength?.IsEnabled = true;
        }

        private async void cmdCreateVault_Click(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrWhiteSpace(txtAppName.Text) &&
                !string.IsNullOrWhiteSpace(txtPasswd.Text) &&
                !string.IsNullOrWhiteSpace(txtUserName.Text) &&
                !txtAppName.Text.Equals("Website/Application Name", StringComparison.Ordinal) &&
                !txtPasswd.Text.Equals("Passwd", StringComparison.Ordinal) &&
                !txtUserName.Text.Equals("User Name / Email", StringComparison.Ordinal)) {

                AppVault v = new AppVault();
                v.AppName = txtAppName.Text.Trim();
                v.UserName = txtUserName.Text.Trim();
                v.Password = txtPasswd.Text.Trim();

                v.IsPasswdSet = true;
                v.IsUserNameSet = true;

                try {
                    long id = await DatabaseHandler.WriteRecordToDatabaseAsync(v);
                    //_vm.TriggerMainWinLoadVaults();
                    MessageBox.Show("Vault entry created successfully.");

                    _refreshAction();
                } catch (Exception ex) {
                    MessageBox.Show($"Failed to create vault entry: {ex.Message}");
                }

                this.Close();       // Close window after creating vault record.

            } else {
                // MESSAGES
            }
        }

        private void ToggleReveal_Click(object sender, RoutedEventArgs e) {
            _showPlain = !_showPlain;       // flip reveal state
            UpdateDisplayedPassword(force: true);
        }
    }
}
