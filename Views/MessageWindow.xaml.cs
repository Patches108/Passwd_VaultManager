using System.Windows;
using System.Windows.Input;

namespace Passwd_VaultManager.Views {
    public partial class MessageWindow : Window {
        public MessageWindow(string txt) {
            InitializeComponent();
            msg.Text = txt;
        }

        private void cmdOkayClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void MessageWindow_Loaded(object sender, RoutedEventArgs e) {
            // Handle ESC to close
            this.PreviewKeyDown += InputDialog_PreviewKeyDown;
            this.Focus();
        }

        private void InputDialog_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                DialogResult = false;
                Close();
            }
        }
    }
}
