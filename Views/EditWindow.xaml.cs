using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Services;
using Passwd_VaultManager.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
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

        //private string _ExChars1 = "";
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
                    _vm.TargetLength = _len;

                    // Optional: if BitRate is already set from generation logic, skip overwrite
                    if (_vm.BitRate <= 0)
                        _vm.BitRate = _bitRate;

                    // 4. Configure slider range
                    sldPasswdLength.Minimum = 8;
                    sldPasswdLength.Value = _passwdWhole.Length;
                    sldPasswdLength.Maximum = _passwdWhole.Length;

                    // Optional: disable slider if password has fixed length (e.g. 128-bit)
                    sldPasswdLength.IsEnabled = _bitRate != 128;

                    _exclOriginalChars = _vm.ExcludedChars;

                    //if(_vm.ExcludedChars.Length > 0)
                    //    _excludeCharCount = _vm.ExcludedChars.Length;

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
                    RefreshPasswordUI();
                    txtPasswd.IsEnabled = false;

                    if (txtAppName.Text.Equals("No App/Account Name"))
                        txtAppName.Text = String.Empty;

                    foreach (var c in _PasswdControls)
                        c.IsEnabled = false;

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
                var helpWin = new Helper("Here, you can adjust the Vault name, User Name/Email, and Password.\n\nClick \'Edit\' checkbox to edit the password value.", SoundController.InfoSound);
                helpWin.Show();
                App.Settings.FirstTimeOpeningEditWin = false;
                SettingsService.Save(App.Settings);
            }
        }

        private void rad_128_Click(object sender, RoutedEventArgs e) {
            _ignoreSliderChange = true;
            _bitRate = 128;
            sldPasswdLength.IsEnabled = true;
            _ignoreSliderChange = false;
        }

        private void rad_256_Click(object sender, RoutedEventArgs e) {
            _ignoreSliderChange = true;
            _bitRate = 256;
            sldPasswdLength.IsEnabled = true;
            _ignoreSliderChange = false;
        }

        private void rad_192_Click(object sender, RoutedEventArgs e) {
            _ignoreSliderChange = true;
            _bitRate = 192;
            sldPasswdLength.IsEnabled = true;
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
            if (_vm is null) return;

            _updating = true;
            try {
                int availableLen;

                // Clamp target BEFORE writing to UI
                txtPasswd.Text = SharedFuncs.BuildDisplay(
                    fullPassword: (_passwdWhole ?? string.Empty).AsSpan(),
                    excludedChars: (_excludedChars ?? string.Empty).AsSpan(),
                    targetLength: _targetLength,
                    availableLen: out availableLen,
                    mask: !_showPlain);

                // Update slider max & value to match new reality
                _ignoreSliderChange = true;
                sldPasswdLength.Maximum = Math.Max(sldPasswdLength.Minimum, availableLen);

                if (_targetLength > sldPasswdLength.Maximum)
                    _targetLength = (int)sldPasswdLength.Maximum;

                sldPasswdLength.Value = _targetLength;
                _ignoreSliderChange = false;

                int shownLen = txtPasswd.Text.Length;

                // VM outputs
                _vm.Length = shownLen;
                _vm.TargetLength = _targetLength;

                int bits = (int)Math.Ceiling(shownLen * 6.285);
                _vm.BitRate = Math.Min(bits, 256);
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

            _excludedChars = NormalizeExcluded(txtCharactersToExclude.Text);
            
            // keep VM in sync for saving
            if (_vm != null)
                _vm.ExcludedChars = _excludedChars;

            if ((_exclOriginalChars.Length > _excludedChars.Length) && (!_exclOriginalChars.Equals("Chars to Exclude"))) {
                var removed = _exclOriginalChars.Except(_excludedChars).ToArray();

                if (removed.Length > 0) {
                    string newStr = new string(removed);
                    txtPasswd.Text += newStr;

                    if(!_passwdWhole.Contains(newStr))
                        _passwdWhole += newStr;

                    int n = CountOccurrences(_passwdWhole, newStr);
                    if (n == 0)
                        _targetLength++;
                    else
                        _targetLength += n;
                }

                _exclOriginalChars = _excludedChars;
            } else {
                if(!txtCharactersToExclude.Text.Trim().Equals("Chars to Exclude"))
                    _exclOriginalChars = txtCharactersToExclude.Text.Trim();
            }


            RefreshPasswordUI();
        }

        private static int CountOccurrences(string source, string value) {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
                return 0;

            int count = 0;
            int index = 0;

            while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) != -1) {
                count++;
                index += value.Length;
            }

            return count;
        }


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
                
                if (!_showPlain) {
                    _showPlain = !_showPlain;       // flip reveal state
                    RefreshPasswordUI();
                    Clipboard.SetText(txtPasswd.Text);
                } else {
                    Clipboard.SetText(txtPasswd.Text);
                }

                _showPlain = !_showPlain;       // flip reveal state
                RefreshPasswordUI();

                new ToastNotification("Text copied to clipboard", true, SoundController.SuccessSound).Show();
            } else {
                new MessageWindow("ERROR: Password field is empty. Generate or manually create a password first", SoundController.ErrorSound).ShowDialog();
            }
        }

        private void cmdManuallyEnterPasswd_Click(object sender, RoutedEventArgs e) {
            sldPasswdLength.IsEnabled = false;

            txtPasswd.IsReadOnly = false;
            lblPasswdStatus.Visibility = Visibility.Hidden;

            txtPasswd.Text = String.Empty;
            txtPasswd.Focus();

            // In manual mode, we no longer use _passwdWhole pipeline.
            _passwdWhole = string.Empty;
            _excludedChars = string.Empty;
            _targetLength = 0;

            _vm.Length = 0;
            _vm.TargetLength = 8;        // or 0 if you allow it, but your slider Min is 8
            _vm.SliderMaxLength = 41;    // default
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

            if(txtPasswd.Text.Trim().Length == 0) {
                new MessageWindow("Password cannot be empty.", SoundController.ErrorSound).Show();
                return;
            }

            if (txtPasswd.Text.Trim().Length < 8) {
                new MessageWindow("Password cannot be less than 8 characters.", SoundController.ErrorSound).Show();
                return;
            }

            // if mask enabled. 1. toggle it off THEN save.
            if (!_showPlain) {
                _showPlain = !_showPlain;       // flip reveal state
                RefreshPasswordUI();
            }

            // Update vault in DB
            try {
                if (String.IsNullOrWhiteSpace(txtAppName.Text))
                    _vm?.AppName = "No App/Account Name";

                int calcBitRate = (int)Math.Ceiling((int)sldPasswdLength.Value * 6.285);

                if (calcBitRate > 256)
                    calcBitRate = 256;

                _vm?.BitRate = calcBitRate;


                await _vm?.SaveAsync();

                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    var toast = new ToastNotification($"Vault entry - ({_vm?.AppName}) - updated successfully.", true, SoundController.SuccessSound);
                    toast.Show();
                });

            } catch (Exception ex) {
                new MessageWindow($"Failed to create vault entry - ({_vm?.AppName}) \n\n {ex.Message}.", SoundController.ErrorSound).Show(); ;
            }

            //App.Settings.FirstTimeOpeningEditWin = false;
            //App.Settings.FirstTimeNewAppName_EditWin = false;
            //App.Settings.FirstTimeNewUserName_EditWin = false;
            //SettingsService.Save(App.Settings);

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

            // if excluded chars in txtCharactersToExclude...
            if (txtCharactersToExclude.Text != "Chars to Exclude" || !string.IsNullOrWhiteSpace(txtCharactersToExclude.Text.Trim())){
                txtCharactersToExclude.Text = "";
            }

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
                _vm.TargetLength = _len;

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

            SharedFuncs.Apply(this, App.Settings);
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

            if (sldPasswdLength.Value <= 8)
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
                var helpWin = new Helper("Here, you can adjust the website/app name.", SoundController.InfoSound);
                helpWin.Show();
                App.Settings.FirstTimeNewAppName_EditWin = false;
                SettingsService.Save(App.Settings);
            }
        }

        private void txtUserName_GotFocus(object sender, RoutedEventArgs e) {
            if (App.Settings.FirstTimeNewUserName_EditWin) {
                var helpWin = new Helper("Here, you can adjust the username/email you will use to log into the website/app.", SoundController.InfoSound);
                helpWin.Show();
                App.Settings.FirstTimeNewUserName_EditWin = false;
                SettingsService.Save(App.Settings);
            }
        }

        private void AppNameHelpMe_Click(object sender, RoutedEventArgs e) {
            var helpWin = new Helper("This is where you edit the Application/website name.", SoundController.InfoSound);
            helpWin.Show();
        }

        private void UsernameHelpMe_Click(object sender, RoutedEventArgs e) {
            var helpWin = new Helper("This is where you edit the User Name/Login/Email for the account.", SoundController.InfoSound);
            helpWin.Show();
        }

        private void EditHelpMe_Click(object sender, RoutedEventArgs e) {
            var helpWin = new Helper("Enable this checkbox to edit the password.\n\nYou can also regenerate a new password.", SoundController.InfoSound);
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

        private static string NormalizeExcluded(string? raw) {
            var s = (raw ?? string.Empty).Trim();
            return s.Equals("Chars to Exclude", StringComparison.Ordinal) ? string.Empty : s;
        }

        private void cmdCopyEmailToClopboard(object sender, RoutedEventArgs e) {
            Clipboard.SetText(txtUserName.Text);
            new ToastNotification("Copied to clipboard", true, SoundController.SuccessSound).Show();
        }
    }
}
