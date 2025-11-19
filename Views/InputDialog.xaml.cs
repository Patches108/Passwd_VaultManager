using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Passwd_VaultManager.Views
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public string UserInput => txtInput.Text.Trim();

        public InputDialog() {
            InitializeComponent();
            this.PreviewKeyDown += InputDialog_PreviewKeyDown;
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void InputDialog_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                DialogResult = false;
                Close();
            }
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                DialogResult = true;
                Close();
            }
        }

        private void frmInputDialog_Loaded(object sender, RoutedEventArgs e) {
            txtInput.Focus();
        }

        // DO I NEED THIS?
        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            var closeBtn = GetTemplateChild("btnClose") as Button;
            if (closeBtn != null) {
                closeBtn.Click += (s, e) => this.Close();
            }
        }
    }
}
