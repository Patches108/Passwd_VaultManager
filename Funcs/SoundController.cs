using System.IO;
using System.Windows;
using System.Windows.Media;


namespace Passwd_VaultManager.Funcs {
    public static class SoundController {

        public static string AudioFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio");

        public static string SuccessSound => Path.Combine(AudioFolder, "success.mp3");
        public static string ErrorSound => Path.Combine(AudioFolder, "error.mp3");
        public static string InfoSound => Path.Combine(AudioFolder, "info.mp3");

        public static void Play(string path) {

            //bool a = !App.Settings.SoundEnabled;
            //bool b = string.IsNullOrWhiteSpace(path);
            //bool c = !File.Exists(path);

            if (!App.Settings.SoundEnabled || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            Application.Current.Dispatcher.BeginInvoke(() => {
                var player = new MediaPlayer();
                player.Open(new Uri(path, UriKind.Absolute));
                player.Volume = 1.0;
                Thread.Sleep(100);  // Thread timing issue - sometimes a little pause is needed to make it play the mp3.
                player.Play();
            });
        }

    }
}
