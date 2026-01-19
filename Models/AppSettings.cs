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




namespace Passwd_VaultManager.Models {

    /// <summary>
    /// Represents application settings including font preferences, sound and speech options, and flags for first-time
    /// usage of various windows.
    /// </summary>
    public class AppSettings {
        public string FontFamily { get; set; } = "Segoe UI";
        public double FontSize { get; set; } = 15;
        public bool SoundEnabled { get; set; } = true;
        public bool SpeechEnabled { get; set; } = true;
        public bool FirstTimeOpeningApp { get; set; } = true;
        public bool FirstTimeOpeningNewWin { get; set; } = true;
        public bool FirstTimeOpeningEditWin { get; set; } = true;

        // New Window Settings
        public bool FirstTimeNewAppName_NewWin { get; set; } = true;
        public bool FirstTimeNewUserName_NewWin { get; set; } = true;
        public bool FirstTimeNewPassword_NewWin { get; set; } = true;

        // Edit Window Settings
        public bool FirstTimeNewAppName_EditWin { get; set; } = true;
        public bool FirstTimeNewUserName_EditWin { get; set; } = true;
        public bool FirstTimeNewPassword_EditWin { get; set; } = true;
    }
}