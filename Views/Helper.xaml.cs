using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for Helper.xaml
    /// </summary>
    public partial class Helper : Window {

        private DispatcherTimer _closeTimer;

        readonly string txt = string.Empty;

        public Helper(string msg) {
            InitializeComponent();
            txt = msg;

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
            _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _closeTimer.Tick += (s2, e2) =>
            {
                _closeTimer.Stop();
                PlayExitAndClose(targetLeft, offscreenRight);
            };
            _closeTimer.Start();
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
        }

        private void cmdX_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
