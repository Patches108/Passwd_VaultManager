// Password Vault Manager
// Copyright © 2026 Max C (aka Finn).
// All rights reserved.
//
// Licensed under the Password Vault Manager Source-Available License.
// Non-commercial use only.
//
// You may view, use, and modify this source code for personal,
// non-commercial purposes. Redistribution (including modified
// versions and compiled binaries) is permitted only if no fee
// is charged and this copyright notice and license are included.
//
// Commercial use, sale of binaries, or distribution for profit
// requires explicit written permission from the copyright holder.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND.
// See the LICENSE file in the project root for full terms.




using System.IO;
using System.Windows;
using System.Windows.Media;


namespace Passwd_VaultManager.Funcs {

    /// <summary>
    /// Provides static methods and properties for playing audio notifications and managing sound file paths within the
    /// application.
    /// </summary>
    public static class SoundController {

        public static string AudioFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio");
        public static string SuccessSound => Path.Combine(AudioFolder, "success.mp3");
        public static string ErrorSound => Path.Combine(AudioFolder, "error.mp3");
        public static string InfoSound => Path.Combine(AudioFolder, "info.mp3");


        /// <summary>
        /// Plays a sound file from the specified path if sound is enabled and the file exists.
        /// </summary>
        /// <param name="path">The absolute path to the sound file to play.</param>
        public static void Play(string path) {

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
