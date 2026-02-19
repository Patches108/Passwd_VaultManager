using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Services;
using Passwd_VaultManager.ViewModels;
using Passwd_VaultManager.Views;
using System.Windows;
using System.Windows.Controls;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for NewWindow.xaml
    /// </summary>
    public partial class NewWindow : Window {

        private readonly NewWindowVM _vm = new();

        private int _bitRate = 256;
        private int _targetLength = 0;        // slider value; we treat 0 as "no limit" OR you can treat it as exact
        private bool _updating;
        private bool _ignoreSliderChange;
        private bool _showPlain;              // false = masked, true = reveal
        private bool ChangesMade = false;

        private string _passwdWhole = string.Empty;   // source password (never mutated by exclusions)
        private string _excludedChars = string.Empty; // current exclusions (normalized)

        private readonly Func<Task> _refreshAction;

        private const double BitsPerChar = 6.285;     // your alphabet's log2 size
        private const int BitsCap = 256;

        public NewWindow(Func<Task> refreshAction) {
            InitializeComponent();
            DataContext = _vm;

            _refreshAction = refreshAction;

            lblPasswdStatus.Visibility = Visibility.Hidden;

            sldPasswdLength.Minimum = 8;
            sldPasswdLength.Maximum = 41;
            sldPasswdLength.IsEnabled = false;

            txtCharactersToExclude.IsEnabled = false;
            txtPasswd.IsReadOnly = true;

            if (App.Settings.FirstTimeOpeningNewWin) {
                var helpWin = new Helper(
                    "Enter the website/app name.\n\nThen enter the username/email you'll use for the website/app.\n\nFinally, click generate password (Recommended) or enter a strong password manually.\n\nAdjust password length with the slider, or exclude characters. Lastly, click the 'Create' button",
                    SoundController.InfoSound);
                helpWin.Show();
                App.Settings.FirstTimeOpeningNewWin = false;
                SettingsService.Save(App.Settings);
            }
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e) {
            if (ChangesMade) {
                YesNoWindow confirm = new YesNoWindow($"You have unsaved changes. Discard changes and close edit form?");
                bool confirmed = confirm.ShowDialog() == true && confirm.YesNoWin_Result;

                if (confirmed)
                    this.Close();

            } else {
                this.Close();
            }
        }

        private void cmdManuallyEnterPasswd_Click(object sender, RoutedEventArgs e) {
            // Manual mode: slider/exclusions disabled; user types actual password
            sldPasswdLength.IsEnabled = false;
            txtCharactersToExclude.IsEnabled = false;

            txtPasswd.IsReadOnly = false;
            lblPasswdStatus.Visibility = Visibility.Hidden;

            txtPasswd.Text = string.Empty;
            txtPasswd.Focus();

            // In manual mode, we no longer use _passwdWhole pipeline.
            _passwdWhole = string.Empty;
            _excludedChars = string.Empty;
            _targetLength = 0;

            _vm.Length = 0;
            _vm.TargetLength = 8;        // or 0 if you allow it, but your slider Min is 8
            _vm.SliderMaxLength = 41;    // default
        }

        // ----------------------------
        // Central pipeline
        // ----------------------------
        private void RecomputeAndApplyUI(bool clampSliderToAvailable = true) {
            if (_updating) return;
            _updating = true;

            try {
                // If no source password yet, keep things calm
                if (string.IsNullOrEmpty(_passwdWhole)) {
                    txtPasswd.Text = string.Empty;

                    _vm.Length = 0;
                    _vm.TargetLength = (int)sldPasswdLength.Minimum;
                    _vm.SliderMaxLength = (int)sldPasswdLength.Maximum;

                    return;
                }

                // Build display and get the true available length after exclusions
                int availableLen;

                txtPasswd.Text = SharedFuncs.BuildDisplay(
                    fullPassword: _passwdWhole.AsSpan(),
                    excludedChars: _excludedChars.AsSpan(),
                    targetLength: _targetLength,
                    availableLen: out availableLen,
                    mask: !_showPlain);

                // Clamp target length to what is actually possible
                if (clampSliderToAvailable && _targetLength > availableLen)
                    _targetLength = availableLen;

                _ignoreSliderChange = true;

                // Slider maximum reflects filtered availability
                sldPasswdLength.Maximum = Math.Max(sldPasswdLength.Minimum, availableLen);

                // Keep target inside slider range
                if (_targetLength < (int)sldPasswdLength.Minimum)
                    _targetLength = (int)sldPasswdLength.Minimum;

                if (_targetLength > (int)sldPasswdLength.Maximum)
                    _targetLength = (int)sldPasswdLength.Maximum;

                sldPasswdLength.Value = _targetLength;

                _ignoreSliderChange = false;

                // VM outputs come from DISPLAYED password
                int shownLen = txtPasswd.Text.Length;

                _vm.Length = shownLen;
                _vm.TargetLength = _targetLength;
                _vm.SliderMaxLength = (int)sldPasswdLength.Maximum;

                // UX toggles
                txtCharactersToExclude.IsEnabled = _showPlain && shownLen > 0;
                //sldPasswdLength.IsEnabled = shownLen >= (int)sldPasswdLength.Minimum && _bitRate != 128;
                //sldPasswdLength.IsEnabled = shownLen > (int)sldPasswdLength.Minimum-1;


                lblPasswdStatus.Visibility = Visibility.Visible;
            } finally {
                _ignoreSliderChange = false;
                _updating = false;
            }
        }


        private static string NormalizeExcluded(string raw) {
            var s = (raw ?? string.Empty).Trim();
            return s.Equals("Chars to Exclude", StringComparison.Ordinal) ? string.Empty : s;
        }

        private static int DefaultLenForBitRate(int br) => br switch {
            <= 128 => 21,
            192 => 31,
            _ => 41
        };

        private void cmdGenPasswd_Click(object sender, RoutedEventArgs e) {
            _updating = true;
            try {
                txtPasswd.IsReadOnly = true;

                // Clear exclusions UI
                txtCharactersToExclude.Text = "Chars to Exclude";
                _excludedChars = string.Empty;

                // Generate new source password
                var gen = new PasswdGen();
                _passwdWhole = gen.GenPassword(bitRate: _bitRate);

                // Default target length for selected bitrate
                _targetLength = DefaultLenForBitRate(_bitRate);

                // Slider baseline
                sldPasswdLength.Minimum = 8;
                sldPasswdLength.Maximum = 41;
                sldPasswdLength.Value = _targetLength;
                sldPasswdLength.IsEnabled = true;

                // Enable controls
                txtCharactersToExclude.IsEnabled = true;
            } finally {
                _updating = false;
                ChangesMade = true;
            }

            RecomputeAndApplyUI();
        }

        private void rad_128_Click(object sender, RoutedEventArgs e) {
            _bitRate = 128;
            sldPasswdLength.IsEnabled = true;

            // If already generated, adjust target
            if (!string.IsNullOrEmpty(_passwdWhole)) {
                _targetLength = 21;
                RecomputeAndApplyUI();
            }
        }

        private void rad_192_Click(object sender, RoutedEventArgs e) {
            _bitRate = 192;
            sldPasswdLength.IsEnabled = true;

            if (!string.IsNullOrEmpty(_passwdWhole)) {
                _targetLength = 31;
                RecomputeAndApplyUI();
            }
        }

        private void rad_256_Click(object sender, RoutedEventArgs e) {
            _bitRate = 256;
            sldPasswdLength.IsEnabled = true;

            if (!string.IsNullOrEmpty(_passwdWhole)) {
                _targetLength = 41;
                RecomputeAndApplyUI();
            }
        }

        private void txtAppName_GotFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtAppName.Text) || txtAppName.Text.Equals("Website/Application Name"))
                txtAppName.Text = String.Empty;

            if (App.Settings.FirstTimeNewAppName_NewWin) {
                var helpWin = new Helper("Here, you enter the website/app name you will use to log into the website/app.", SoundController.InfoSound);
                helpWin.Show();
                App.Settings.FirstTimeNewAppName_NewWin = false;
                SettingsService.Save(App.Settings);
            }
        }

        private void txtUserName_GotFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtUserName.Text) || txtUserName.Text.Equals("User Name / Email"))
                txtUserName.Text = String.Empty;

            if (App.Settings.FirstTimeNewUserName_NewWin) {
                var helpWin = new Helper("Here, enter the username/email you will use to log into the website/app.", SoundController.InfoSound);
                helpWin.Show();
                App.Settings.FirstTimeNewUserName_NewWin = false;
                SettingsService.Save(App.Settings);
            }
        }

        private void txtPasswd_GotFocus(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(txtPasswd.Text) || txtPasswd.Text.Equals("Passwd"))
                txtPasswd.Text = String.Empty;

            if (App.Settings.FirstTimeNewPassword_NewWin) {
                var helpWin = new Helper("Here, generate a password (Recommended) or enter a strong password manually.\n\nYou can adjust password length with the slider and by entering characters to exclude. When finished, click the \'Create\' button", SoundController.InfoSound);
                helpWin.Show();
                App.Settings.FirstTimeNewPassword_NewWin = false;
                SettingsService.Save(App.Settings);
            }
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

        private void cmdCopyToClopboard(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrWhiteSpace(txtPasswd.Text)) {
                if (!_showPlain) {
                    _showPlain = !_showPlain;       // flip reveal state
                    RecomputeAndApplyUI();
                    Clipboard.SetText(txtPasswd.Text);
                } else {
                    Clipboard.SetText(txtPasswd.Text);
                }
                new ToastNotification("Text copied to clipboard", true, SoundController.SuccessSound).Show();

                _showPlain = !_showPlain;       // flip reveal state
                RecomputeAndApplyUI();

            } else {
                new MessageWindow("ERROR: Password field is empty.", SoundController.ErrorSound).ShowDialog();
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

        // ----------------------------
        // Slider + exclusions (inputs)
        // ----------------------------
        private void sldPasswdLength_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (_ignoreSliderChange) return;
            if (_updating) return;
            if (string.IsNullOrEmpty(_passwdWhole)) return; // no generated password to adjust

            _targetLength = (int)Math.Round(e.NewValue);
            RecomputeAndApplyUI();
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
            if (string.IsNullOrEmpty(_passwdWhole)) return; // only applies to generated passwords

            _excludedChars = NormalizeExcluded(txtCharactersToExclude.Text);
            RecomputeAndApplyUI();
        }

        // ----------------------------
        // Manual password entry updates VM directly (no pipeline)
        // ----------------------------
        private void txtPasswd_TextChanged(object sender, TextChangedEventArgs e) {
            if (_updating) return;

            if (!txtPasswd.IsReadOnly) {
                int len = txtPasswd.Text.Trim().Length;
                _vm.Length = len;

                int bits = (int)Math.Ceiling(len * BitsPerChar);
                _vm.Length = len;
            }

            ChangesMade = true;
        }

        // ----------------------------
        // Create vault
        // ----------------------------
        private async void cmdCreateVault_Click(object sender, RoutedEventArgs e) {
            // Validate placeholders
            if (string.IsNullOrWhiteSpace(txtAppName.Text) ||
                string.IsNullOrWhiteSpace(txtUserName.Text) ||
                string.IsNullOrWhiteSpace(txtPasswd.Text) ||
                txtAppName.Text.Equals("Website/Application Name", StringComparison.Ordinal) ||
                txtUserName.Text.Equals("User Name / Email", StringComparison.Ordinal) ||
                txtPasswd.Text.Equals("Passwd", StringComparison.Ordinal)) {
                new MessageWindow("App name, User name/Email, and Password fields cannot be empty.",
                    SoundController.ErrorSound).ShowDialog();
                return;
            }

            int len = txtPasswd.Text.Trim().Length;
            if (len < 8) {
                new MessageWindow("Length must be at least 8 characters.", SoundController.ErrorSound).Show();
                return;
            }

            // Ensure saving plain text if you require it (your current design)
            if (!_showPlain && !txtPasswd.IsReadOnly) {
                // Manual mode: already plain
            } else if (!_showPlain && !string.IsNullOrEmpty(_passwdWhole)) {
                // Generated mode: temporarily reveal for saving the real chars
                _showPlain = true;
                RecomputeAndApplyUI();
            }

            // Calculate bit rate from actual length (displayed/saved)
            int calcBitRate = (int)Math.Ceiling(len * BitsPerChar);
            calcBitRate = Math.Min(calcBitRate, BitsCap);

            var v = new AppVault {
                AppName = txtAppName.Text.Trim(),
                UserName = txtUserName.Text.Trim(),
                Password = txtPasswd.Text.Trim(),
                ExcludedChars = NormalizeExcluded(txtCharactersToExclude.Text),

                IsPasswdSet = true,
                IsUserNameSet = true,
                BitRate = calcBitRate
            };

            try {
                await DatabaseHandler.WriteRecordToDatabaseAsync(v);

                Application.Current.Dispatcher.Invoke(() => {
                    var toast = new ToastNotification(
                        $"Vault entry - ({v.AppName}) - created successfully.",
                        true,
                        SoundController.SuccessSound);
                    toast.Show();
                });

                await _refreshAction();
            } catch (Exception ex) {
                new MessageWindow($"Failed to create vault entry - ({v.AppName})\n\n{ex.Message}.",
                    SoundController.ErrorSound).ShowDialog();
                return;
            }

            //SettingsService.Save(App.Settings);

            ChangesMade = false;

            Close();
        }

        private void ToggleReveal_Click(object sender, RoutedEventArgs e) {
            _showPlain = !_showPlain;
            RecomputeAndApplyUI();
        }

        private void AppNameHelpMe_Click(object sender, RoutedEventArgs e) {
            var helpWin = new Helper("This is where you enter the application/website name. i.e, Gmail, Youtube, etc...", SoundController.InfoSound);
            helpWin.Show();
        }

        private void PasswordHelpMe_Click(object sender, RoutedEventArgs e) {
            var helpWin = new Helper("It is recommended you generate a password with the \'Generate\' button with bit rate to 256.\n\nAdjust length and characters as needed with the slider and \'Exclude Characters\' controls.", SoundController.InfoSound);
            helpWin.Show();
        }

        private void UsernameHelpMe_Click(object sender, RoutedEventArgs e) {
            var helpWin = new Helper("Here, you enter the user name you use to login.\n\nUsually this is a custom username or an email.", SoundController.InfoSound);
            helpWin.Show();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            var closeBtn = GetTemplateChild("btnClose") as Button;
            if (closeBtn != null) {
                closeBtn.Click += (s, e) => this.Close();
            }
        }

        private void NewWin_Loaded(object sender, RoutedEventArgs e) {
            SharedFuncs.Apply(this, App.Settings);
            ChangesMade = false;
        }

        private void txtAppName_TextChanged(object sender, TextChangedEventArgs e) {
            ChangesMade = true;
        }

        private void txtUserName_TextChanged(object sender, TextChangedEventArgs e) {
            ChangesMade = true;
        }

        private void cmdCopyEmailToClopboard(object sender, RoutedEventArgs e) {
            Clipboard.SetText(txtUserName.Text);
            new ToastNotification("Copied to clipboard", true, SoundController.SuccessSound).Show();
        }
    }
}
