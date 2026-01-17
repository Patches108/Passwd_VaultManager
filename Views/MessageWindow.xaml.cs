using Passwd_VaultManager.Funcs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Passwd_VaultManager.Views {
    public partial class MessageWindow : Window {

        private string _soundPath = String.Empty;

        public MessageWindow(string txt, string soundPath) {
            InitializeComponent();
            msg.Text = txt;
            _soundPath = soundPath;
        }

        private void cmdOkayClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void MessageWindow_Loaded(object sender, RoutedEventArgs e) {
            // Handle ESC to close
            this.PreviewKeyDown += InputDialog_PreviewKeyDown;
            this.Focus();

            SharedFuncs.Apply(this, App.Settings);

            SoundController.Play(_soundPath);
        }

        private void InputDialog_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                DialogResult = false;
                Close();
            }
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
