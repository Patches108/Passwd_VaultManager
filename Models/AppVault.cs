using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Policy;

namespace Passwd_VaultManager.Models
{
    public class AppVault : INotifyPropertyChanged {
        
        private string _appName;
        private string _password;
        private string _userName;

        private int _bitRate;

        private bool _userNameSet;
        private bool _passwdSet;
        private bool _statusOkay;

        private readonly Guid AppVaultGUID;  // To store class instance identifier.

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        
        public AppVault() {
            AppVaultGUID = Guid.NewGuid();
        }

        /// <summary>
        /// Getters and setters
        /// </summary>
        /// 
        public Guid getAppVaultInstanceGuid { get { return AppVaultGUID; } }

        public DateTime? DateCreated { get; set; }

        public int BitRate {
            get => _bitRate;
            set {
                if (_bitRate == value) return;
                _bitRate = value;
                OnPropertyChanged();
            }
        }

        public string AppName {
            get => _appName;
            set {
                if (_appName == value) return;
                // Add try catches with help window later...
                _appName = ValidateString(value, nameof(value)); 
                OnPropertyChanged(); 
            }
        }

        public string Password {
            get => _password;
            set {
                if (_password == value) return;
                _password = ValidateString(value, nameof(value)); 
                OnPropertyChanged(); 
            }
        }

        public string UserName {
            get => _userName;
            set {
                if (_userName == value) return;
                _userName = ValidateString(value, nameof(value)); 
                OnPropertyChanged(); 
            }
        }

        public bool IsUserNameSet {
            get => _userNameSet;
            set {
                if (_userNameSet == value) return;
                _userNameSet = value; 
                OnPropertyChanged(); 
            }
        }

        public bool IsPasswdSet {
            get => _passwdSet;
            set {
                if (_passwdSet == value) return;
                _passwdSet = value; 
                OnPropertyChanged(); 
            }
        }

        public bool IsStatusGood {
            get => _statusOkay;
            set {
                if (_statusOkay == value) return;
                _statusOkay = value;
                OnPropertyChanged();
            }
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
