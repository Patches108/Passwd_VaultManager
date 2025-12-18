using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Services;
using Passwd_VaultManager.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
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
        private bool _ignoreSliderChange;

        private string _passwdWhole = String.Empty;     // password
        private string _excludedChars = String.Empty;   // current exclusions from textbox
        private string _exclOriginalChars = String.Empty;   // original exclusions for comparison

        public EditWindow()
        {
            InitializeComponent();
            // get vm when it actually exists
            DataContextChanged += (_, e) => _vm = e.NewValue as EditWindowVM;

            _updating = true;

            Loaded += (_, __) =>
            {
                if (_vm != null) {

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
                    sldPasswdLength.Maximum = _passwdWhole.Length+ _vm.ExcludedChars.Length;
                    sldPasswdLength.Value = _passwdWhole.Length;

                    // Optional: disable slider if password has fixed length (e.g. 128-bit)
                    sldPasswdLength.IsEnabled = _bitRate != 128;

                    _exclOriginalChars = _vm.ExcludedChars;

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
                    //UpdateDisplayedPassword(force: true);
                    RefreshPasswordUI();
                    txtPasswd.IsEnabled = false;

                    ChangesMade = false;

                } else {
                    // error msg
                }
                    _updating = false; // allow UpdateDisplayedPassword to run later

                // Reflect bitrate value.
                // if neither, nothing should be selected.
                switch (_vm?.BitRate) {
                    case <= 128: rdo_128.IsChecked = true; break;
                    case 192: rdo_192.IsChecked = true; break;
                    case 256: rdo_256.IsChecked = true; break;
                }
            };

            lblPasswdStatus.IsEnabled = false;

            if (App.Settings.FirstTimeOpeningEditWin) {
                var helpWin = new Helper("Here, you can adjust the Vault name, User Name/Email, and Password.\n\nClick \'Edit\' checkbox to edit the password value.");
                helpWin.Show();
            }
        }

        private void rad_128_Click(object sender, RoutedEventArgs e) {
            _ignoreSliderChange = true;
            _bitRate = 128;
            sldPasswdLength.IsEnabled = true;
            //sldPasswdLength.Value = (double)21;
            _ignoreSliderChange = false;
        }

        private void rad_256_Click(object sender, RoutedEventArgs e) {
            _ignoreSliderChange = true;
            _bitRate = 256;
            sldPasswdLength.IsEnabled = true;
            //sldPasswdLength.Value = (double)41;
            _ignoreSliderChange = false;
        }

        private void rad_192_Click(object sender, RoutedEventArgs e) {
            _ignoreSliderChange = true;
            _bitRate = 192;
            sldPasswdLength.IsEnabled = true;
            //sldPasswdLength.Value = (double)31;
            _ignoreSliderChange = false;
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

        private void RefreshPasswordUI() {
            if (_updating) return;

            _updating = true;
            try {
                // clamp target to valid range but DON'T overwrite it from txtPasswd.Text length
                int maxLen = _passwdWhole?.Length ?? 0;
                if (_targetLength < 0) _targetLength = 0;
                if (_targetLength > maxLen) _targetLength = maxLen;

                // rebuild display strictly from the full password
                txtPasswd.Text = SharedFuncs.BuildDisplay(
                    fullPassword: (_passwdWhole ?? string.Empty).AsSpan(),
                    excludedChars: (_excludedChars ?? string.Empty).AsSpan(),
                    targetLength: _targetLength,
                    currentText: txtPasswd.Text.AsSpan(),
                    force: false,
                    placeholder: "Passwd".AsSpan(),
                    mask: !_showPlain);

                int shownLen = txtPasswd.Text.Trim().Length;

                // Update VM from the derived display
                _vm?.Length = shownLen;
                _vm?.SliderValue = _targetLength.ToString();
                _vm?.BitRate = shownLen;
                // BitRate should NOT be set to length unless that's intentional
                // (your current code does this, but it's likely wrong)
                // _vm?.BitRate = ???; // leave to your radio buttons

                // Keep slider range stable: based on the whole password, not current display
                sldPasswdLength.Maximum = maxLen;

                // Update slider without causing binding issues
                _ignoreSliderChange = true;
                sldPasswdLength.Value = _targetLength;
                _ignoreSliderChange = false;

                // enable/disable exclude based on slider value if you want
                txtCharactersToExclude.IsEnabled = _targetLength > 8;
            } finally {
                _updating = false;
            }
        }


        private void sldPasswdLength_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (_ignoreSliderChange) return;
            if (_updating) return;

            _targetLength = (int)Math.Round(e.NewValue);
            RefreshPasswordUI();
        }


        //private void sldPasswdLength_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {

        //    if (sldPasswdLength.Value <= (int)8)
        //        txtCharactersToExclude.IsEnabled = false;

        //    if (_ignoreSliderChange) return;
        //    if (_updating) return;

        //    _targetLength = (int)Math.Round(e.NewValue);
        //    _vm?.SliderValue = _targetLength.ToString();
        //    _vm?.BitRate = _targetLength;            

        //    UpdateDisplayedPassword();

        //    _vm?.Length = txtPasswd.Text.Trim().Length; // update after display updated
        //}


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

            _excludedChars = (txtCharactersToExclude.Text ?? string.Empty).Trim();

            if (_excludedChars.Equals("Chars to Exclude", StringComparison.Ordinal))
                _excludedChars = string.Empty;

            if(_excludedChars.Length < _exclOriginalChars.Length) { // true if chars erased from excluded textfield.
                string removed = new string(_exclOriginalChars.Except(_excludedChars).ToArray());
                
                Debug.WriteLine(removed);
                Debug.WriteLine(_passwdWhole);
                _targetLength++;
                _exclOriginalChars = _excludedChars;
                _passwdWhole += removed;
                Debug.WriteLine(_passwdWhole);
            }
            RefreshPasswordUI();
        }

        //private void txtCharactersToExclude_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
        //    if (_updating) return;

        //    _excludedChars = txtCharactersToExclude.Text ?? string.Empty;

        //    // Ignore placeholder text
        //    if (_excludedChars.Trim().Equals("Chars to Exclude", StringComparison.Ordinal))
        //        _excludedChars = string.Empty;

        //    // BUG: 1. Open 1st record. 2. Slider down to anything. 3. erase charstoexclude 4. slider down again. 5. not chars to exclude do not reflect properly.
        //    if ((txtCharactersToExclude.Text.Trim() != "" || !string.IsNullOrEmpty(_exclOriginalChars)) && _exclOriginalChars != "Chars to Exclude") {
        //        if (_exclOriginalChars != _excludedChars) {
        //            for (int i = 0; i < _exclOriginalChars.Length; i++) {
        //                if (!_excludedChars.Contains(_exclOriginalChars[i])) {
        //                    txtPasswd.Text += _exclOriginalChars[i];
        //                    _passwdWhole = txtPasswd.Text.Trim();
        //                    _exclOriginalChars = _excludedChars.Trim();
        //                }
        //            }
        //        }
        //    }

        //    int len = txtPasswd.Text.Trim().Length;
        //    _vm?.Length = len;

        //    // BUG: _targetLength seems to be causing an issue here.
        //    //      _targetLength not updating when chars erased from txtexcludechars
        //    //      try adding chars back THEN recalc len.
        //    _targetLength = len;
        //    _vm?.SliderValue = len.ToString();
        //    sldPasswdLength.Maximum = len;
        //    sldPasswdLength.Value = (double)len;

        //    //sldPasswdLength.Value = (double)_vm?.Length;

        //    UpdateDisplayedPassword();

        //    Debug.WriteLine("");
        //    Debug.WriteLine("*************txtCharactersToExclude*******************");
        //    Debug.WriteLine("_excludedChars / \ttxtCharactersToExclude / \t\ttxtPasswd.Text / \t\t_passwdWhole / \t\t_targetLength");
        //    Debug.WriteLine($"{_excludedChars} \t\t\t/ \t\t\t\t{txtCharactersToExclude.Text.Trim()} / \t\t\t\t\t{txtPasswd.Text} / \t{_passwdWhole} / \t{_targetLength}");
        //    Debug.WriteLine("******************************************************");
        //    Debug.WriteLine("");
        //}

        //private void UpdateDisplayedPassword(bool force = false) {
        //    if (_updating) return;

        //    _updating = true;

        //    //_targetLength = _passwdWhole.Length;

        //    try {
        //        txtPasswd.Text = SharedFuncs.BuildDisplay(
        //            fullPassword: _passwdWhole.AsSpan(),
        //            excludedChars: _excludedChars.AsSpan(),
        //            targetLength: _targetLength,
        //            currentText: txtPasswd.Text.AsSpan(),
        //            force: false,
        //            placeholder: "Passwd".AsSpan(),
        //            mask: !_showPlain);
        //    } finally {
        //        _updating = false;
        //    }
        //}

        private void txtPasswd_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            if(_enableCharsExcludeSwitch)
                if (txtPasswd.Text.Trim().Length > 0)
                    txtCharactersToExclude?.IsEnabled = true;
            

            if (txtPasswd.Text.Trim().Length >= 8)
                sldPasswdLength?.IsEnabled = true;

            ChangesMade = true;
        }

        private void cmdCopyToClopboard(object sender, RoutedEventArgs e) {
            // Copy password text field data to clipboard
            if (!String.IsNullOrWhiteSpace(txtPasswd.Text.Trim())) {
                Clipboard.SetText(txtPasswd.Text);
                new ToastNotification("Text copied to clipboard", true, SoundController.SuccessSound).Show();
            } else {
                new MessageWindow("ERROR: Password field is empty. Generate or manually create a password first", SoundController.ErrorSound).ShowDialog();
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

            // if mask enabled. 1. toggle it off THEN save.
            if (!_showPlain) {
                _showPlain = !_showPlain;       // flip reveal state
                //UpdateDisplayedPassword(force: true);
                RefreshPasswordUI();
            }

            // Update vault in DB
            try {
                await _vm?.SaveAsync();

                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    var toast = new ToastNotification($"Vault entry - ({_vm?.AppName}) - updated successfully.", true, SoundController.SuccessSound);
                    toast.Show();
                });

            } catch (Exception ex) {
                new MessageWindow($"Failed to create vault entry - ({_vm?.AppName}) \n\n {ex.Message}.", SoundController.ErrorSound);
            }

            App.Settings.FirstTimeOpeningEditWin = false;
            App.Settings.FirstTimeNewAppName_EditWin = false;
            App.Settings.FirstTimeNewUserName_EditWin = false;
            SettingsService.Save(App.Settings);

            ChangesMade = false;

            this.Close();
        }

        private void txtAppName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            ChangesMade = true;
        }

        private void txtUserName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            ChangesMade = true;
        }

        private void cmdGenPasswd_Click(object sender, RoutedEventArgs e) {
            if (_vm is null) return;

            if (rdo_128.IsChecked == true)
                _bitRate = 128;
            else if (rdo_192.IsChecked == true)
                _bitRate = 192;
            if (rdo_256.IsChecked == true)
                _bitRate = 256;

            _updating = true;
            try {
                txtCharactersToExclude.IsEnabled = true;
                sldPasswdLength.IsEnabled = true;

                var gen = new Passwd_VaultManager.Funcs.PasswdGen();
                string pw = gen.GenPassword(bitRate: _bitRate);

                _passwdWhole = pw;

                switch (_bitRate) {
                    case 128: _len = 21; sldPasswdLength.Value = _len; break;
                    case 192: _len = 31; sldPasswdLength.Value = _len; break;
                    case 256: _len = 41; sldPasswdLength.Value = _len; break;
                }

                _vm.BitRate = _bitRate;
                _vm.Length = _len;
                _targetLength = _len;
                sldPasswdLength.Value = _len;
                _vm.SliderValue = _len.ToString();

            } finally { 
                _updating = false; 
            }

            //UpdateDisplayedPassword(force: true);
            RefreshPasswordUI();
            lblPasswdStatus.Visibility = Visibility.Visible;
        }

        private void NewWindow_Loaded(object sender, RoutedEventArgs e) {
            sldPasswdLength.IsEnabled = false;
            txtCharactersToExclude.IsEnabled = false;
        }

        private void ToggleReveal_Click(object sender, RoutedEventArgs e) {
            _showPlain = !_showPlain;       // flip reveal state
            //UpdateDisplayedPassword(force: true);
            RefreshPasswordUI();

            txtCharactersToExclude.IsEnabled = _showPlain;
            sldPasswdLength.IsEnabled = _showPlain;
        }

        private void chk_EditPassword_Checked(object sender, RoutedEventArgs e) {
            // enable the password controls.
            foreach (var c in _PasswdControls)
                c.IsEnabled = true;

            _enableCharsExcludeSwitch = true;

            if (sldPasswdLength.Value == 8)
                txtCharactersToExclude.IsEnabled = false;
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

        protected override void OnClosing(CancelEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            var closeBtn = GetTemplateChild("btnClose") as Button;
            if (closeBtn != null) {
                closeBtn.Click += (s, e) => this.Close();
            }
        }
    }
}
