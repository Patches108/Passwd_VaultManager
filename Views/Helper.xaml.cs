using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Services;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for Helper.xaml
    /// </summary>
    public partial class Helper : Window {

        private DispatcherTimer _closeTimer;

        readonly string txt = string.Empty;

        private string _soundPath = String.Empty;

        SpeechSynthesizer synth = new SpeechSynthesizer();

        public Helper(string msg, string sPath) {
            InitializeComponent();
            txt = msg;
            _soundPath = sPath;

            // final resting position
            double targetLeft = SystemParameters.WorkArea.Width - Width - 20;
            double offscreenRight = SystemParameters.WorkArea.Width + Width;

            Top = SystemParameters.WorkArea.Height - Height - 40;

            // start off-screen
            Left = offscreenRight;

            // play slide-in animation
            Storyboard enter = (Storyboard)FindResource("EnterStoryboard");

            // tell the animation where to end
            foreach (var t in enter.Children.OfType<DoubleAnimation>()) {
                if (Storyboard.GetTargetProperty(t).Path == "Left")
                    t.From = offscreenRight;
                t.To = targetLeft;
            }

            BeginStoryboard(enter);

            PlaySpeech();

            // close after timeout with slide-out
            _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _closeTimer.Tick += (s2, e2) =>
            {
                _closeTimer.Stop();
                PlayExitAndClose(targetLeft, offscreenRight);
            };
            _closeTimer.Start();

            if(!String.IsNullOrWhiteSpace(_soundPath))
                SoundController.Play(_soundPath);
        }

        private void PlayExitAndClose(double startLeft, double offscreenRight) {
            var exit = (Storyboard)FindResource("ExitStoryboard");
            foreach (var t in exit.Children.OfType<DoubleAnimation>()) {
                if (Storyboard.GetTargetProperty(t).Path == "Left") {
                    t.From = startLeft;
                    t.To = offscreenRight;
                }
            }

            exit.Completed += (_, __) => Close();
            BeginStoryboard(exit);
        }

        private void frmHelper_Loaded(object sender, RoutedEventArgs e) {
            txtMessage.Text = txt;
            SharedFuncs.Apply(this, App.Settings);
            //bool b = App.Settings.SpeechEnabled;
            toggleSpeech.IsChecked = App.Settings.SpeechEnabled;
        }

        private void cmdX_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Sound_On(object sender, RoutedEventArgs e) {
            App.Settings.SpeechEnabled = true;
            SettingsService.Save(App.Settings);
            PlaySpeech();
        }

        private void Sound_Off(object sender, RoutedEventArgs e) {
            App.Settings.SpeechEnabled = false;
            SettingsService.Save(App.Settings);
            StopSpeech();
        }
        private void PlaySpeech() {
            // Speech controls...
            if (App.Settings.SpeechEnabled) {
                synth.SelectVoiceByHints(
                    VoiceGender.Female,
                    VoiceAge.Teen
                );

                synth.Rate = 0;
                synth.Volume = 90;

                synth.SpeakAsync(txt);
            }
        }

        private void StopSpeech() {
            try {
                synth.SpeakAsyncCancelAll();
            } catch {
                // do nothing?
            }
        }

        protected override void OnClosed(EventArgs e) {
            StopSpeech();
            synth.Dispose();
            base.OnClosed(e);
        }

    }
}
