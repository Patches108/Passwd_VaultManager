using Passwd_VaultManager.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Passwd_VaultManager.Funcs;

namespace Passwd_VaultManager.Models
{
    public class AppVault : INotifyPropertyChanged {
        
        private string _appName;
        private string _password;
        private string _userName;
        private string _excludes;

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

        public string ExcludedChars {
            get => _excludes;
            set {
                if (_excludes == value) return;

                try {
                    string validated = Funcs.SharedFuncs.ValidateString(value, nameof(value));
                    _excludes = validated;
                    OnPropertyChanged();
                } catch (Exception ex) {
                    new MessageWindow($"ERROR: {ex.Message}").ShowDialog();
                }
            }
        }

        public int BitRate {
            get => _bitRate;
            set {
                if (_bitRate == value) return;

                if (Funcs.SharedFuncs.ValidateNumeral(value))   // <-- validate value, not _bitRate
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
                    string validated = Funcs.SharedFuncs.ValidateString(value, nameof(value));
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
                    string validated = Funcs.SharedFuncs.ValidateString(value, nameof(value));
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
                    string validated = Funcs.SharedFuncs.ValidateString(value, nameof(value));
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
    }
}
