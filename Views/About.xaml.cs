using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Text;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window {
        public About() {
            InitializeComponent();
        }

        private void About_Loaded(object sender, RoutedEventArgs e) {
            this.PreviewKeyDown += InputDialog_PreviewKeyDown;
            this.Focus();

            // Load license data from file.
            string licenseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Licenses");
            string[] fileList = Directory.GetFiles(licenseFolder);

            StringBuilder sb = new StringBuilder();

            foreach (string file in fileList) {
                
                string[] lines = File.ReadAllLines(file);
                
                foreach (string line in lines) 
                    sb.AppendLine(line);

                BetweenFiles(sb);
            }

            msg.Text = sb.ToString();
        }

        private void BetweenFiles(StringBuilder sb) {
            for (int i = 0; i < 3; i++)     // 3 NEW LINES BETWEEN FILES.
                sb.AppendLine();

            sb.AppendLine("--------------------------------");

            for (int i = 0; i < 3; i++)     // 3 NEW LINES BETWEEN FILES.
                sb.AppendLine();
        }


        private void InputDialog_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                DialogResult = false;
                Close();
            }
        }

        private void cmdCopyToClipboard_Click(object sender, RoutedEventArgs e) {
            Clipboard.SetText(msg.Text);
            new ToastNotification("Text copied to clipboard!", true).Show();
        }

        private void cmdOkayClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
