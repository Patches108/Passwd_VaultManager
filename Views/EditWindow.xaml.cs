using Passwd_VaultManager.Models;
using Passwd_VaultManager.ViewModels;
using System.Text;
using System.Windows;

namespace Passwd_VaultManager.Views
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        private EditWindowVM? _vm;

        private bool ChangesMade = false;

        private int _bitRate = 256;
        private int _len = 41;
        private int _targetLength = 0;        // current slider length (0 = no limit)

        private bool _updating;

        private string _passwdWhole = String.Empty;     // the generated full password
        private string _excludedChars = String.Empty;   // current exclusions from textbox

        public EditWindow()
        {
            InitializeComponent();
            // get vm when it actually exists
            DataContextChanged += (_, e) => _vm = e.NewValue as EditWindowVM;

            Loaded += (_, __) =>
            {
                if (_vm != null) {
                    // seed the pipeline from the VM
                    _passwdWhole = _vm.Password ?? string.Empty;

                    // if you must set the textbox here, do it once:
                    txtPasswd.Text = _passwdWhole;
                }

                _updating = false; // allow UpdateDisplayedPassword to run later
            };

            _updating = true; // block changes until Loaded

            lblPasswdStatus.Visibility = Visibility.Hidden;
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

        private void rad_192_Click(object sender, RoutedEventArgs e) {
            _bitRate = 192;
            sldPasswdLength.IsEnabled = true;
            sldPasswdLength.Value = (double)31;
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
            if(_vm is not null)
                _vm.SliderValue = _targetLength.ToString();

            UpdateDisplayedPassword();
        }

        private void txtCharactersToExclude_GotFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtCharactersToExclude.Text) || txtCharactersToExclude.Text.Equals("Chars to Exclude"))
                txtCharactersToExclude.Text = String.Empty;
        }

        private void txtCharactersToExclude_LostFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtCharactersToExclude.Text))
                txtCharactersToExclude.Text = "Chars to Exclude";
        }

        private void txtCharactersToExclude_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            if (_updating) return;

            _excludedChars = txtCharactersToExclude.Text ?? string.Empty;

            // Ignore placeholder text
            if (_excludedChars.Trim().Equals("Chars to Exclude", StringComparison.Ordinal))
                _excludedChars = string.Empty;

            UpdateDisplayedPassword();
        }

        private void UpdateDisplayedPassword(bool force = false) {
            if (_updating) return;

            _updating = true;
            try {
                string s = _passwdWhole ?? string.Empty;

                // Apply exclusions
                if (!string.IsNullOrEmpty(_excludedChars)) {
                    var exclude = new HashSet<char>(_excludedChars);
                    var sb = new StringBuilder(s.Length);
                    foreach (char c in s)
                        if (!exclude.Contains(c)) sb.Append(c);
                    s = sb.ToString();
                }

                // Apply length
                if (_targetLength > 0 && s.Length > _targetLength)
                    s = s.Substring(0, _targetLength);

                // Write result. The “Passwd” placeholder guard is what stopped your first click.
                if (force || !string.Equals(txtPasswd.Text, "Passwd", StringComparison.Ordinal))
                    txtPasswd.Text = s;
            } finally {
                _updating = false;
            }
        }

        private void txtPasswd_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            if (txtPasswd.Text.Trim().Length > 0)
                txtCharactersToExclude?.IsEnabled = true;

            if (txtPasswd.Text.Trim().Length >= 16)
                sldPasswdLength?.IsEnabled = true;

            ChangesMade = true;
        }

        private void cmdCopyToClopboard(object sender, RoutedEventArgs e) {
            // Copy password text field data to clipboard
            if (!String.IsNullOrWhiteSpace(txtPasswd.Text.Trim()))
                Clipboard.SetText(txtPasswd.Text);
            else {
                new MessageWindow("ERROR: Password field is empty. Generate or manually create a password first").ShowDialog();
            }
        }

        private void cmdManuallyEnterPasswd_Click(object sender, RoutedEventArgs e) {
            sldPasswdLength.IsEnabled = false;

            lblPasswdStatus.Visibility = Visibility.Hidden;

            txtPasswd.Text = String.Empty;
            txtPasswd.Focus();
        }

        private void EditWin_cmdClose_Click(object sender, RoutedEventArgs e) {
            if (ChangesMade) {
                // DO YOU WANT TO SAVE?
            } else {
                this.Close();
            }
        }

        private void EditWin_cmdUpdateVault_Click(object sender, RoutedEventArgs e) {
            //

            ChangesMade = false;
        }

        private void txtAppName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            ChangesMade = true;
        }

        private void txtUserName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            ChangesMade = true;
        }

        private void cmdGenPasswd_Click(object sender, RoutedEventArgs e) {
            if (_vm is null) return;

            _updating = true;
            try {
                txtCharactersToExclude.IsEnabled = true;
                sldPasswdLength.IsEnabled = true;

                var gen = new Passwd_VaultManager.Funcs.PasswdGen();
                string pw = gen.GenPassword(bitRate: _bitRate);

                _passwdWhole = pw;

                switch (_bitRate) {
                    case 128: _len = 21; sldPasswdLength.IsEnabled = false; break;
                    case 192: _len = 31; sldPasswdLength.IsEnabled = true; break;
                    case 256: _len = 41; sldPasswdLength.IsEnabled = true; break;
                }

                _vm.BitRate = _bitRate;
                _vm.Length = _len;
                _targetLength = _len;
                sldPasswdLength.Value = _len;
                _vm.SliderValue = _len.ToString();

            } finally { 
                _updating = false; 
            }

            UpdateDisplayedPassword(force: true);
            lblPasswdStatus.Visibility = Visibility.Visible;
        }

        private void NewWindow_Loaded(object sender, RoutedEventArgs e) {
            //_updating = false;
            //txtPasswd.IsEnabled = false;
            sldPasswdLength.IsEnabled = false;
            txtCharactersToExclude.IsEnabled = false;
        }
    }
}
