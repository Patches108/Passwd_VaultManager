﻿using Passwd_VaultManager.Models;
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
        
        private string _passwdWhole = String.Empty;     // the generated full password
        private string _excludedChars = String.Empty;   // current exclusions from textbox
        


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
            
            txtCharactersToExclude.IsEnabled = true;
            sldPasswdLength.IsEnabled = true;
            
            var gen = new Passwd_VaultManager.Funcs.PasswdGen();
            string pw = gen.GenPassword(bitRate: _bitRate); // returns at least 21 chars here
            
            txtPasswd.Text = pw;

            _vm.BitRate = _bitRate;

            switch(_bitRate) {
                case 128:
                    _len = 21;
                    sldPasswdLength.Value = (double)21;
                    sldPasswdLength.IsEnabled = false;
                    break;
                case 192:
                    _len = 31;
                    sldPasswdLength.IsEnabled = true;
                    sldPasswdLength.Value = (double)31;
                    break;
                case 256:
                    _len = 41;
                    sldPasswdLength.IsEnabled = true;
                    sldPasswdLength.Value = (double)41;
                    break;
            }

            _vm.Length = _len;
            
            _passwdWhole = txtPasswd.Text.Trim();

            lblPasswdStatus.Visibility = Visibility.Visible;
        }

        private async Task cmdCreateVault_ClickAsync(object sender, RoutedEventArgs e) {
            // 1. Checks for and disallows empty controls.
            if(String.IsNullOrWhiteSpace(txtAppName.Text) || String.IsNullOrWhiteSpace(txtPasswd.Text) || String.IsNullOrWhiteSpace(txtUserName.Text)) {
                // Display error screen
                return;
            }

            // 2. Creates vault.
            AppVault vault = new AppVault();
            vault.AppName = txtAppName.Text;
            vault.UserName = txtUserName.Text;
            vault.Password = txtPasswd.Text;
            
            vault.IsPasswdSet = true;
            vault.IsUserNameSet = true;
            vault.IsStatusGood = true;

            // 3. Saves it to DB.
            await DatabaseHandler.WriteRecordToDatabaseAsync(vault);

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
            if (_updating) return;

            _targetLength = (int)Math.Round(e.NewValue);
            _vm.SliderValue = _targetLength.ToString();

            UpdateDisplayedPassword();
        }

        private void UpdateDisplayedPassword() {
            _updating = true;
            try {
                // 1) Start from the full generated password
                string s = _passwdWhole ?? string.Empty;

                // 2) Apply exclusions
                if (!string.IsNullOrEmpty(_excludedChars)) {
                    var exclude = new HashSet<char>(_excludedChars);
                    var filtered = new StringBuilder(s.Length);
                    foreach (char c in s)
                        if (!exclude.Contains(c))
                            filtered.Append(c);
                    s = filtered.ToString();
                }

                // 3) Apply length limit
                if (_targetLength > 0 && s.Length > _targetLength)
                    s = s.Substring(0, _targetLength);

                // 4) Ignore placeholder “Passwd”
                if (!string.Equals(txtPasswd.Text, "Passwd", StringComparison.Ordinal))
                    txtPasswd.Text = s;
            } finally {
                _updating = false;
            }
        }
        //private void sldPasswdLength_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {

        //    if (String.IsNullOrWhiteSpace(txtPasswd.Text)) return;

        //    int roundedValue = (int)sldPasswdLength.Value;
        //    _vm.SliderValue = roundedValue.ToString();

        //    //txtPasswd.Text = _passwdWhole[..roundedValue];
        //    if(_passwdWhole.Length > 0 && roundedValue <= _passwdWhole.Length)
        //        txtPasswd.Text = _passwdWhole[..roundedValue];
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

            _excludedChars = txtCharactersToExclude.Text ?? string.Empty;

            // Ignore placeholder text
            if (_excludedChars.Trim().Equals("Chars to Exclude", StringComparison.Ordinal))
                _excludedChars = string.Empty;

            UpdateDisplayedPassword();
        }

        //private void txtCharactersToExclude_TextChanged(object sender, TextChangedEventArgs e) {
        //    if (_updating) return;

        //    if (!string.IsNullOrWhiteSpace(txtPasswd.Text) &&
        //        txtCharactersToExclude.Text.Trim() != "Chars to Exclude") {
        //        _updating = true;
        //        try {
        //            if (!string.IsNullOrWhiteSpace(txtPasswd.Text) && !txtPasswd.Text.Trim().Equals("Passwd", StringComparison.Ordinal)) {
        //                var excluded = txtCharactersToExclude.Text;
        //                var s = _passwdWhole;
        //                txtPasswd.Text = new string(s.Where(c => !excluded.Contains(c)).ToArray());
        //            }
        //        } finally {
        //            _updating = false;
        //        }
        //    }
        //}

        private void txtPasswd_TextChanged(object sender, TextChangedEventArgs e) {

            if (txtPasswd.Text.Trim().Length > 0)
                txtCharactersToExclude?.IsEnabled = true;

            if (txtPasswd.Text.Trim().Length >= 16)
                sldPasswdLength?.IsEnabled = true;
        }

        private async void cmdCreateVault_Click(object sender, RoutedEventArgs e) {
            if(!String.IsNullOrWhiteSpace(txtAppName.Text.Trim()) &&
                !String.IsNullOrWhiteSpace(txtPasswd.Text.Trim()) &&
                !String.IsNullOrWhiteSpace(txtUserName.Text.Trim())) {

                AppVault v = new AppVault();
                v.AppName = txtAppName.Text.Trim();
                v.UserName = txtUserName.Text.Trim();
                v.Password = txtPasswd.Text.Trim();

                try {
                    long id = await DatabaseHandler.WriteRecordToDatabaseAsync(v);
                    MessageBox.Show("Vault entry created successfully.");
                } catch (Exception ex) {
                    MessageBox.Show($"Failed to create vault entry: {ex.Message}");
                }


            }
        }
    }
}
