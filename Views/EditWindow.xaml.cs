using Passwd_VaultManager.Models;
using Passwd_VaultManager.ViewModels;
using Passwd_VaultManager.Funcs;
using System.Windows;
using System.Windows.Controls;

namespace Passwd_VaultManager.Views
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        private EditWindowVM? _vm;

        List<FrameworkElement> _PasswdControls = new();

        private bool ChangesMade = false;

        private int _bitRate = 256;
        private int _len = 41;
        private int _targetLength = 0;        // current slider length (0 = no limit)

        private bool _updating;
        private bool _showPlain;   // false = masked, true = reveal

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
                    _passwdWhole = _vm.Password ?? string.Empty;
                    txtPasswd.Text = _passwdWhole;

                    // Add password controls to list for later iteration as per checkbox
                    _PasswdControls.Add(txtPasswd);
                    _PasswdControls.Add(lblPasswd);
                    _PasswdControls.Add(lblPasswdStatus);
                    _PasswdControls.Add(lblBitRateLabel);
                    _PasswdControls.Add(rdo_128);
                    _PasswdControls.Add(rdo_192);
                    _PasswdControls.Add(rdo_256);
                    _PasswdControls.Add(Img_TogglePasswdMask);
                    _PasswdControls.Add(txtCharactersToExclude);
                    _PasswdControls.Add(sldPasswdLength);
                    _PasswdControls.Add(cmdGenPasswd);
                    _PasswdControls.Add(cmdManuallyEnterPasswd);
                    _PasswdControls.Add(cmdManuallyEnterPasswd);
                    //lblPasswdSliderValue
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

            if (_vm is not null) {
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
            }

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

        private void ToggleReveal_Click(object sender, RoutedEventArgs e) {
            _showPlain = !_showPlain;       // flip reveal state
            UpdateDisplayedPassword(force: true);
        }

        private void chk_EditPassword_Checked(object sender, RoutedEventArgs e) {
            // enable the password controls.
            foreach (var c in _PasswdControls)
                c.IsEnabled = true;
        }

        private void EditPassword_Unchecked(object sender, RoutedEventArgs e) {
            // disable the password controls.
            foreach (var c in _PasswdControls)
                c.IsEnabled = false;
        }
    }
}
