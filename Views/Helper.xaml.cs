using System.Windows;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for Helper.xaml
    /// </summary>
    public partial class Helper : Window {

        readonly string txt = string.Empty;

        public Helper(string msg) {
            InitializeComponent();
            txt = msg;
        }

        private void frmHelper_Loaded(object sender, RoutedEventArgs e) {
            txtMessage.Text = txt;
        }
    }
}
