using Passwd_VaultManager.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
        public long Id { get; set; }

        public Guid getAppVaultInstanceGuid { get { return AppVaultGUID; } }

        public DateTime? DateCreated { get; set; }

        public int BitRate {
            get => _bitRate;
            set {
                if (_bitRate == value) return;

                if (ValidateNumeral(value))   // <-- validate value, not _bitRate
                {
                    _bitRate = value;
                    OnPropertyChanged();
                } else {
                    new MessageWindow("ERROR: Bit Rate must be between 8 and 256");
                }
            }
        }

        public string AppName {
            get => _appName;
            set {
                if (_appName == value) return;

                try {
                    string validated = ValidateString(value, nameof(value));
                    _appName = validated;
                    OnPropertyChanged();
                } catch (Exception ex) {
                    new MessageWindow($"ERROR: {ex.Message}").ShowDialog();
                }
            }
        }


        public string Password {
            get => _password;
            set {
                if (_password == value) return;


                try {
                    string validated = ValidateString(value, nameof(value));
                    _password = validated;
                    OnPropertyChanged();
                } catch (Exception ex) {
                    new MessageWindow($"ERROR: {ex.Message}").ShowDialog();
                }
            }
        }

        public string UserName {
            get => _userName;
            set {
                if (_userName == value) return;

                try {
                    string validated = ValidateString(value, nameof(value));
                    _userName = validated;
                    OnPropertyChanged();
                } catch (Exception ex) {
                    new MessageWindow($"ERROR: {ex.Message}").ShowDialog();
                }
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

        private static bool ValidateNumeral(int val) { 
            return val >= 8 && val <= 256;
        }
    }
}
