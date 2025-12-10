using System.IO;
using System.Windows.Media;

namespace Passwd_VaultManager.Funcs {
    public static class SoundController {

        public static string AudioFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio");

        public static string SuccessSound => Path.Combine(AudioFolder, "positive_sound.mp3");
        public static string ErrorSound => Path.Combine(AudioFolder, "error_sound.mp3");
        public static string InfoSound => Path.Combine(AudioFolder, "info_sound.mp3");
        public static string WarningSound => Path.Combine(AudioFolder, "warning_sound.mp3");
        public static string HelpSound => Path.Combine(AudioFolder, "help_sound.mp3");

        public static void Play(string path) {

            if (!App.Settings.SoundEnabled || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            var player = new MediaPlayer();
            player.Open(new Uri(path, UriKind.Absolute));
            player.Volume = 1.0;
            player.Play();
        }

    }
}
