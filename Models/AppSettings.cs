namespace Passwd_VaultManager.Models {
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