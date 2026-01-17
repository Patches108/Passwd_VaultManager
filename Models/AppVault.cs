using System.ComponentModel;
using System.Runtime.CompilerServices;
using Passwd_VaultManager.Funcs;

namespace Passwd_VaultManager.Models {
    public class AppVault : INotifyPropertyChanged {
        public const string DefaultNoName = "No App/Account Name";

        private string _appName = string.Empty;
        private string _password = string.Empty;
        private string _userName = string.Empty;
        private string _excludedChars = string.Empty;

        private int _bitRate;

        private bool _isAppNameSet;
        private bool _isUserNameSet;
        private bool _isPasswdSet;
        private bool _isStatusGood;

        public event PropertyChangedEventHandler? PropertyChanged;

        public AppVault() {
            InstanceGuid = Guid.NewGuid();
        }

        // ---- Identity / metadata ----
        public long Id { get; set; }
        public Guid InstanceGuid { get; }
        public DateTime? DateCreated { get; set; }

        // ---- Core fields ----
        public string AppName {
            get => _appName;
            set => SetValidatedString(ref _appName, value);
        }

        public string UserName {
            get => _userName;
            set => SetValidatedString(ref _userName, value);
        }

        public string Password {
            get => _password;
            set => SetValidatedString(ref _password, value);
        }

        public string ExcludedChars {
            get => _excludedChars;
            set => SetValidatedString(ref _excludedChars, value);
        }

        public int BitRate {
            get => _bitRate;
            set {
                if (_bitRate == value) return;

                // validate value, not current field
                if (!SharedFuncs.ValidateNumeral(value))
                    throw new ArgumentOutOfRangeException(nameof(BitRate), "Bit Rate must be between 8 and 256.");

                _bitRate = value;
                OnPropertyChanged();
            }
        }

        // ---- Flags ----
        public bool IsAppNameSet {
            get => _isAppNameSet;
            set => SetField(ref _isAppNameSet, value);
        }

        public bool IsUserNameSet {
            get => _isUserNameSet;
            set => SetField(ref _isUserNameSet, value);
        }

        public bool IsPasswdSet {
            get => _isPasswdSet;
            set => SetField(ref _isPasswdSet, value);
        }

        public bool IsStatusGood {
            get => _isStatusGood;
            set => SetField(ref _isStatusGood, value);
        }

        // ---- Convenience ----
        public void SetNoName() => AppName = DefaultNoName;

        // ---- Helpers ----
        private void SetValidatedString(ref string field, string? value, [CallerMemberName] string? propName = null) {
            value ??= string.Empty;
            if (field == value) return;

            // Keep model pure: validate or throw, don't show dialogs here.
            field = SharedFuncs.ValidateString(value, propName ?? "Value");
            OnPropertyChanged(propName);
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propName = null) {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propName);
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

//using Passwd_VaultManager.Views;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using Passwd_VaultManager.Funcs;

//namespace Passwd_VaultManager.Models
//{
//    public class AppVault : INotifyPropertyChanged {

//        private string _appName;
//        private string _password;
//        private string _userName;
//        private string _excludes;

//        private int _bitRate;

//        private bool _appNameSet;
//        private bool _userNameSet;
//        private bool _passwdSet;
//        private bool _statusOkay;

//        private readonly Guid AppVaultGUID;  // To store class instance identifier.

//        public event PropertyChangedEventHandler? PropertyChanged;

//        private void OnPropertyChanged([CallerMemberName] string name = null)
//            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



//        public AppVault() {
//            AppVaultGUID = Guid.NewGuid();
//        }

//        /// <summary>
//        /// Getters and setters
//        /// </summary>
//        /// 
//        public long Id { get; set; }

//        public Guid getAppVaultInstanceGuid { get { return AppVaultGUID; } }

//        public DateTime? DateCreated { get; set; }

//        public void SetNoNameError() {
//            AppName = "No App/Account Name";
//        }

//        public string ExcludedChars {
//            get => _excludes;
//            set {
//                if (_excludes == value) return;

//                try {
//                    string validated = Funcs.SharedFuncs.ValidateString(value, nameof(value));
//                    _excludes = validated;
//                    OnPropertyChanged();
//                } catch (Exception ex) {
//                    new MessageWindow($"ERROR:\n\n{ex.Message}", SoundController.ErrorSound ).ShowDialog();
//                }
//            }
//        }

//        public int BitRate {
//            get => _bitRate;
//            set {
//                if (_bitRate == value) return;

//                if (Funcs.SharedFuncs.ValidateNumeral(value))   // <-- validate value, not _bitRate
//                {
//                    _bitRate = value;
//                    OnPropertyChanged();
//                } else {
//                    new MessageWindow("ERROR: Bit Rate must be between 8 and 256", SoundController.ErrorSound);
//                }
//            }
//        }

//        public string AppName {
//            get => _appName;
//            set {
//                if (_appName == value) return;

//                try {
//                    string validated = Funcs.SharedFuncs.ValidateString(value, nameof(value));
//                    _appName = validated;
//                    OnPropertyChanged();
//                } catch (Exception ex) {
//                    new MessageWindow($"ERROR:\n\n{ex.Message}", SoundController.ErrorSound).ShowDialog();
//                }
//            }
//        }


//        public string Password {
//            get => _password;
//            set {
//                if (_password == value) return;


//                try {
//                    string validated = Funcs.SharedFuncs.ValidateString(value, nameof(value));
//                    _password = validated;
//                    OnPropertyChanged();
//                } catch (Exception ex) {
//                    new MessageWindow($"ERROR:\n\n{ex.Message}", SoundController.ErrorSound).ShowDialog();
//                }
//            }
//        }

//        public string UserName {
//            get => _userName;
//            set {
//                if (_userName == value) return;

//                try {
//                    string validated = Funcs.SharedFuncs.ValidateString(value, nameof(value));
//                    _userName = validated;
//                    OnPropertyChanged();
//                } catch (Exception ex) {
//                    new MessageWindow($"ERROR:\n\n{ex.Message}", SoundController.ErrorSound).ShowDialog();
//                }
//            }
//        }

//        public bool IsAppNameSet {
//            get => _appNameSet;
//            set {
//                if (_appNameSet == value) return;
//                _appNameSet = value; 
//                OnPropertyChanged(); 
//            }
//        }

//        public bool IsUserNameSet {
//            get => _userNameSet;
//            set {
//                if (_userNameSet == value) return;
//                _userNameSet = value; 
//                OnPropertyChanged(); 
//            }
//        }

//        public bool IsPasswdSet {
//            get => _passwdSet;
//            set {
//                if (_passwdSet == value) return;
//                _passwdSet = value; 
//                OnPropertyChanged(); 
//            }
//        }

//        public bool IsStatusGood {
//            get => _statusOkay;
//            set {
//                if (_statusOkay == value) return;
//                _statusOkay = value;
//                OnPropertyChanged();
//            }
//        }
//    }
//}
