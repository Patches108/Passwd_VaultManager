using Passwd_VaultManager.Funcs;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            var sb = new StringBuilder();

            foreach (string file in fileList) {
                string[] lines = File.ReadAllLines(file);

                string? title = null;
                string? copyright = null;
                string? webLink = null;
                var body = new StringBuilder();

                foreach (string rawLine in lines) {
                    string line = rawLine.Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Detect the "special" lines once
                    if (title == null &&
                        (line.StartsWith("MIT License", StringComparison.OrdinalIgnoreCase) ||
                         line.StartsWith("The MIT License", StringComparison.OrdinalIgnoreCase))) {
                        title = line;
                        continue;
                    }

                    if (copyright == null &&
                        line.StartsWith("Copyright", StringComparison.OrdinalIgnoreCase)) {
                        copyright = line;
                        continue;
                    }

                    if (webLink == null &&
                        (line.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                         line.StartsWith("https://", StringComparison.OrdinalIgnoreCase))) {
                        webLink = line;
                        continue;
                    }

                    // Everything else: one big paragraph
                    if (body.Length > 0)
                        body.Append(' ');
                    body.Append(line);
                }

                // Now output in a fixed, nice format

                if (title != null) {
                    sb.AppendLine(title);
                    sb.AppendLine();               // blank line after title
                }

                if (copyright != null) {
                    sb.AppendLine(copyright);
                    sb.AppendLine();               // blank line after copyright
                }

                if (body.Length > 0) {
                    sb.AppendLine(body.ToString()); // whole paragraph
                    sb.AppendLine();                // blank line before link
                }

                if (webLink != null) {
                    sb.AppendLine(webLink);        // link on its own line
                    sb.AppendLine();               // trailing blank line
                }

                BetweenFiles(sb);
            }

            sb.Remove(sb.Length - 40, 32); // remove last '------------------------------'

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
            new ToastNotification("Text copied to clipboard!", true, SoundController.SuccessSound).Show();
        }

        private void cmdOkayClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
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
