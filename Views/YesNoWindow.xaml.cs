using System.Windows;
using System.Windows.Input;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for YesNoWindow.xaml
    /// </summary>
    public partial class YesNoWindow : Window {

        public bool YesNoWin_Result { get; private set; }
        public YesNoWindow(string txt) {
            InitializeComponent();
            msg.Text = txt;
        }

        private void YesNoWin_Loaded(object sender, RoutedEventArgs e) {
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

        private void cmdYes_Click(object sender, RoutedEventArgs e) {
            YesNoWin_Result = true;
            DialogResult = true;
        }

        private void cmdNo_Click(object sender, RoutedEventArgs e) {
            YesNoWin_Result = false;
            DialogResult = false;
        }
    }
}
