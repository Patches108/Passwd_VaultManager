using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Services;
using Passwd_VaultManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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
        private bool _enableCharsExcludeSwitch = false;

        private int _bitRate = 256;
        private int _len = 41;
        private int _targetLength = 0;        // current slider length (0 = no limit)

        private bool _updating;
        private bool _showPlain = false;   // false = masked, true = reveal

        private string _passwdWhole = String.Empty;     // password
        private string _excludedChars = String.Empty;   // current exclusions from textbox

        public EditWindow()
        {
            InitializeComponent();
            // get vm when it actually exists
            DataContextChanged += (_, e) => _vm = e.NewValue as EditWindowVM;

            _updating = true;

            Loaded += (_, __) =>
            {
                if (_vm != null) {
                    //_passwdWhole = _vm.Password ?? string.Empty;
                    //txtPasswd.Text = _passwdWhole;

                    //_len = _passwdWhole.Length;

                    //_vm.BitRate = _bitRate;
                    //_vm.Length = _len;
                    //_targetLength = _len;
                    //sldPasswdLength.Value = _len;
                    //_vm.SliderValue = _len.ToString();

                    //_passwdWhole = txtPasswd.Text.Trim();
                    //sldPasswdLength.Maximum = _passwdWhole.Length;
                    //sldPasswdLength.Value = _passwdWhole.Length;


                    // 1. Get full password text
                    _passwdWhole = _vm.Password?.Trim() ?? string.Empty;

                    // 2. Sync password display
                    txtPasswd.Text = _passwdWhole;

                    // 3. Cache and propagate length + bitrate
                    _len = _passwdWhole.Length;
                    _targetLength = _len;
                    _vm.Length = _len;
                    _vm.SliderValue = _len.ToString();

                    // Optional: if BitRate is already set from generation logic, skip overwrite
                    if (_vm.BitRate <= 0)
                        _vm.BitRate = _bitRate;

                    // 4. Configure slider range
                    sldPasswdLength.Minimum = 8;
                    sldPasswdLength.Maximum = _passwdWhole.Length;
                    sldPasswdLength.Value = _passwdWhole.Length;

                    // Optional: disable slider if password has fixed length (e.g. 128-bit)
                    sldPasswdLength.IsEnabled = _bitRate != 128;


                    // Add password controls to list for later iteration as per checkbox
                    _PasswdControls.Add(txtPasswd);
                    _PasswdControls.Add(lblPasswd);
                    _PasswdControls.Add(lblPasswdStatus);
                    _PasswdControls.Add(lblBitRateLabel);
                    _PasswdControls.Add(rdo_128);
                    _PasswdControls.Add(rdo_192);
                    _PasswdControls.Add(rdo_256);
                    _PasswdControls.Add(Img_TogglePasswdMask);
                    _PasswdControls.Add(cmdToggleReveal);
                    _PasswdControls.Add(txtCharactersToExclude);
                    _PasswdControls.Add(sldPasswdLength);
                    _PasswdControls.Add(cmdGenPasswd);
                    _PasswdControls.Add(cmdManuallyEnterPasswd);
                    _PasswdControls.Add(lblPasswdSliderValue);
                    _PasswdControls.Add(cmdCopyToClipboard);

                    txtPasswd.IsEnabled = true;
                    _showPlain = !_showPlain;       // flip reveal state
                    UpdateDisplayedPassword(force: true);
                    txtPasswd.IsEnabled = false;

                    ChangesMade = false;

                }
                _updating = false; // allow UpdateDisplayedPassword to run later
            };

            lblPasswdStatus.Visibility = Visibility.Hidden;

            //App.Settings.FirstTimeOpeningEditWin = true;       // REMOVE THIS IN PROD

            if (App.Settings.FirstTimeOpeningEditWin) {
                var helpWin = new Helper("Here, you can adjust the Vault name, User Name/Email, and Password.\n\nClick \'Edit\' checkbox to edit the password value.");
                helpWin.Show();
            }
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
                _vm.Length = _targetLength;

                // Update bitrate dynamically
                switch (_vm.Length) {
                    case >= 41:
                        _vm.BitRate = 256;
                        break;
                    case >= 31:
                        _vm.BitRate = 192;
                        break;
                    case >= 21:
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
            if(_enableCharsExcludeSwitch)
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
                
                // confirm with yes/no dialog
                YesNoWindow confirm = new YesNoWindow($"You have unsaved changes. Discard changes and close edit form?");
                bool confirmed = confirm.ShowDialog() == true && confirm.YesNoWin_Result;

                if (confirmed) 
                    this.Close();   

            } else {
                this.Close();
            }
        }

        private async void EditWin_cmdUpdateVault_Click(object sender, RoutedEventArgs e) {

            // Update vault in DB
            try {
                await _vm.SaveAsync();

                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    var toast = new ToastNotification($"Vault entry - ({_vm.AppName}) - created successfully.", true);
                    toast.Show();
                });

            } catch (Exception ex) {
                new MessageWindow($"Failed to create vault entry - ({_vm.AppName}) \n\n {ex.Message}.");
            }

            App.Settings.FirstTimeOpeningEditWin = false;
            App.Settings.FirstTimeNewAppName_EditWin = false;
            App.Settings.FirstTimeNewUserName_EditWin = false;
            SettingsService.Save(App.Settings);

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

            _enableCharsExcludeSwitch = true;
        }

        private void EditPassword_Unchecked(object sender, RoutedEventArgs e) {
            // disable the password controls.
            foreach (var c in _PasswdControls)
                c.IsEnabled = false;

            _enableCharsExcludeSwitch = false;
        }

        private void txtAppName_GotFocus(object sender, RoutedEventArgs e) {
            if (App.Settings.FirstTimeNewAppName_EditWin) {
                // FIX THIS MESSAGE
                var helpWin = new Helper("To make a Vault, enter the website/app name.\n\nThen enter the username/email you will use to log into the website/app.\n\nFinally, click generate password (Recommended) or enter a strong password manually.\n\nYou can adjust password length with the slider and by entering characters to exclude. When you're finished, click the \'Create\' button");
                helpWin.Show();
            }
        }

        private void txtUserName_GotFocus(object sender, RoutedEventArgs e) {
            if (App.Settings.FirstTimeNewUserName_EditWin) {
                // FIX THIS MESSAGE
                var helpWin = new Helper("To make a Vault, enter the website/app name.\n\nThen enter the username/email you will use to log into the website/app.\n\nFinally, click generate password (Recommended) or enter a strong password manually.\n\nYou can adjust password length with the slider and by entering characters to exclude. When you're finished, click the \'Create\' button");
                helpWin.Show();
            }
        }

        private void AppNameHelpMe_Click(object sender, RoutedEventArgs e) {
            var helpWin = new Helper("This is where you edit the Application/website name.");
            helpWin.Show();
        }

        private void UsernameHelpMe_Click(object sender, RoutedEventArgs e) {
            var helpWin = new Helper("This is where you edit the User Name/Login/Email for the account.");
            helpWin.Show();
        }

        private void EditHelpMe_Click(object sender, RoutedEventArgs e) {
            var helpWin = new Helper("Enable this checkbox to edit the password.\n\nYou can also regenerate a new password.");
            helpWin.Show();
        }
    }
}
