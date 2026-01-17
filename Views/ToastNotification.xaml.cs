using Passwd_VaultManager.Funcs;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for ToastNotification.xaml
    /// </summary>
    public partial class ToastNotification : Window {

        private DispatcherTimer _closeTimer;
        private string _soundPath = String.Empty;

        public ToastNotification(string message, bool state, string sPath) {
            InitializeComponent();
            txtMessage.Text = message;
            _soundPath = sPath;

            Loaded += (s, e) =>
            {
                // Load correct Image
                if (state)
                    imgState.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Success_Icon.png"));
                else
                    imgState.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Failure_Icon.png"));


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

                // close after timeout with slide-out
                _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.5) };
                _closeTimer.Tick += (s2, e2) =>
                {
                    _closeTimer.Stop();
                    PlayExitAndClose(targetLeft, offscreenRight);
                };
                _closeTimer.Start();

                SoundController.Play(_soundPath);
            };
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

        private void frmToast_Loaded(object sender, RoutedEventArgs e) {
            SharedFuncs.Apply(this, App.Settings);
        }

        //private void PlayNotificationSound(string track) {
        //    try {
        //        var player = new MediaPlayer();
        //        var audioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio", track);

        //        if (File.Exists(audioPath)) {
        //            player.Open(new Uri(audioPath));
        //            player.Volume = 1.0;
        //            player.Play();
        //        } else {
        //            Debug.WriteLine("⚠️ Audio file not found: " + audioPath);
        //        }
        //    } catch (Exception ex) {
        //        Debug.WriteLine("⚠️ Failed to play audio: " + ex.Message);
        //    }
        //}
    }
}
