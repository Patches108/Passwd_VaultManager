using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Passwd_VaultManager.Funcs;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for pin.xaml
    /// </summary>
    public partial class pin : Window {

        private int _failedAttempts = 0;
        private const int MaxAttempts = 5;
        
        private static readonly Regex _digits = new(@"^\d$", RegexOptions.Compiled); // Digits only

        public pin() {
            InitializeComponent();
            Loaded += (_, __) => tb1.Focus();
        }

        private void PinBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !_digits.IsMatch(e.Text ?? "");
        }

        // Auto-advance + Enable Proceed when all 4 filled
        private void PinBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (sender is TextBox tb) {
                if (tb.Text.Length == 1) {
                    if (tb == tb1) tb2.Focus();
                    else if (tb == tb2) tb3.Focus();
                    else if (tb == tb3) tb4.Focus();
                }
            }

            btnProceed.IsEnabled = AreAllFilled();
        }

        // Backspace to previous box when empty
        private void PinBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (sender is not TextBox tb) return;

            if (e.Key == Key.Back && string.IsNullOrEmpty(tb.Text)) {
                if (tb == tb4) tb3.Focus();
                else if (tb == tb3) tb2.Focus();
                else if (tb == tb2) tb1.Focus();
            }
        }

        private bool AreAllFilled() =>
            tb1.Text.Length == 1 && tb2.Text.Length == 1 && tb3.Text.Length == 1 && tb4.Text.Length == 1;

        // make the handler async so we can await Task.Delay
        private async void btnProceed_Click(object sender, RoutedEventArgs e) {
            var pin = $"{tb1.Text}{tb2.Text}{tb3.Text}{tb4.Text}";
            if (pin.Length != 4)  // sanity guard
                return;

            // Verify against stored encrypted PIN
            if (PinStorage.VerifyPin(pin)) {
                _failedAttempts = 0;        // reset on success (optional)
                DialogResult = true;        // caller can check this
                Close();
                return;
            }

            // incorrect
            _failedAttempts++;

            new MessageWindow($"Incorrect PIN. Attempt {_failedAttempts} of {MaxAttempts}.", SoundController.ErrorSound).ShowDialog();

            ClearInputs();      // this should: clear all 4, focus tb1, disable Proceed
            txtPrompt.Text = "Incorrect PIN. Try again.";

            if (_failedAttempts >= MaxAttempts) {
                new MessageWindow($"Too many failed attempts. The application will now close.", SoundController.ErrorSound).ShowDialog();
                
                // no UI freeze
                await Task.Delay(300);
                Application.Current.Shutdown();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void ClearInputs() {
            tb1.Clear(); tb2.Clear(); tb3.Clear(); tb4.Clear();
            tb1.Focus();
            btnProceed.IsEnabled = false;
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
