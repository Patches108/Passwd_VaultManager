using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Passwd_VaultManager.Models
{
    public class AppVault : INotifyPropertyChanged {
        private string _appName;
        private string _password;
        private string _userName;

        private bool _userNameSet;
        private bool _passwdSet;
        private bool _statusOkay;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        /// <summary>
        /// Initializes a new instance of the <see cref="PasswdPanelVM"/> class with default values.
        /// </summary>
        /// <remarks>The constructor sets the <see cref="_appName"/>, <see cref="_password"/>, and <see
        /// cref="_userName"/> properties to empty strings. Additionally, the <see cref="_userNameSet"/> and <see
        /// cref="_passwdSet"/> flags are initialized to <see langword="false"/>.</remarks>
        public AppVault() {
            //_appName = String.Empty;
            //_password = String.Empty;
            //_userName = String.Empty;

            //_userNameSet = false;
            //_passwdSet = false;
        }

        /// <summary>
        /// Getters and setters
        /// </summary>
        public string AppName {
            get => _appName;
            set { _appName = ValidateString(value, nameof(value)); OnPropertyChanged(); }
        }

        public string Password {
            get => _password;
            set { _password = ValidateString(value, nameof(value)); OnPropertyChanged(); }
        }

        public string UserName {
            get => _userName;
            set { _userName = ValidateString(value, nameof(value)); OnPropertyChanged(); }
        }

        public bool IsUserNameSet {
            get => _userNameSet;
            set { _userNameSet = value; OnPropertyChanged(); }
        }

        public bool IsPasswdSet {
            get => _passwdSet;
            set { _passwdSet = value; OnPropertyChanged(); }
        }

        public bool IsStatusGood {
            get => _statusOkay;
            set { _statusOkay = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Validates string input, throws exception is invalid.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="PropName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static string ValidateString(string value, string PropName) {

            var s = value?.Trim() ?? throw new ArgumentNullException(PropName);

            if (s.Length == 0) throw new ArgumentException("Value cannot be empty.", PropName);
            if (s.Length > 100) throw new ArgumentOutOfRangeException("Value cannot exceed 100 characters.", PropName);
            if (s.Any(char.IsControl)) throw new ArgumentException("App Name cannot contain controls or escape characters - Use numbers and letters only.", PropName);

            return s;
        }
    }
}
