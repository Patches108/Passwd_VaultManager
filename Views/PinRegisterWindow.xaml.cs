using Passwd_VaultManager.Funcs;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for PinRegisterWindow.xaml
    /// </summary>
    public partial class PinRegisterWindow : Window {
        public PinRegisterWindow() {
            InitializeComponent();
            Loaded += (_, __) => tb1a.Focus();
        }

        // digits-only
        private static readonly Regex Digits = new(@"^\d$", RegexOptions.Compiled);

        private void PinBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
            => e.Handled = !Digits.IsMatch(e.Text ?? "");

        private void PinBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (sender is not TextBox tb) return;

            if (e.Key == Key.Back && string.IsNullOrEmpty(tb.Text)) {
                // smart backspace within row
                switch (tb.Name) {
                    case "tb4a": tb3a.Focus(); break;
                    case "tb3a": tb2a.Focus(); break;
                    case "tb2a": tb1a.Focus(); break;
                    case "tb4b": tb3b.Focus(); break;
                    case "tb3b": tb2b.Focus(); break;
                    case "tb2b": tb1b.Focus(); break;
                }
            }
        }

        // row 1 auto-advance + validation
        private void Pin1_TextChanged(object sender, TextChangedEventArgs e) {
            if (sender is TextBox tb && tb.Text.Length == 1) {
                if (tb == tb1a) tb2a.Focus();
                else if (tb == tb2a) tb3a.Focus();
                else if (tb == tb3a) tb4a.Focus();
                else if (tb == tb4a) tb1b.Focus(); // jump to confirm row start
            }
            Validate();
        }

        // row 2 auto-advance + validation
        private void Pin2_TextChanged(object sender, TextChangedEventArgs e) {
            if (sender is TextBox tb && tb.Text.Length == 1) {
                if (tb == tb1b) tb2b.Focus();
                else if (tb == tb2b) tb3b.Focus();
                else if (tb == tb3b) tb4b.Focus();
            }
            Validate();
        }

        private string PinA => $"{tb1a.Text}{tb2a.Text}{tb3a.Text}{tb4a.Text}";
        private string PinB => $"{tb1b.Text}{tb2b.Text}{tb3b.Text}{tb4b.Text}";

        private bool IsComplete(string s) => s?.Length == 4;

        private void Validate() {
            bool filledA = IsComplete(PinA);
            bool filledB = IsComplete(PinB);

            if (!filledA || !filledB) {
                btnRegister.IsEnabled = false;
                txtStatus.Visibility = Visibility.Collapsed;
                return;
            }

            if (PinA == PinB) {
                btnRegister.IsEnabled = true;
                txtStatus.Visibility = Visibility.Collapsed;
            } else {
                btnRegister.IsEnabled = false;
                txtStatus.Text = "PINs do not match.";
                txtStatus.Visibility = Visibility.Visible;
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e) {
            if (!IsComplete(PinA) || PinA != PinB) {
                Validate();
                return;
            }

            // Save securely (salted hash in %APPDATA%\DreamRecorder\pin.dat)
            PinStorage.SetPin(PinA);

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            var closeBtn = GetTemplateChild("btnClose") as Button;
            if (closeBtn != null) {
                closeBtn.Click += (s, e) => this.Close();
            }
        }

        private void fmrPINReg_Loaded(object sender, RoutedEventArgs e) {
            SharedFuncs.Apply(this, App.Settings);
        }
    }
}
