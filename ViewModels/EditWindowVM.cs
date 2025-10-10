using Passwd_VaultManager.Models;
using Passwd_VaultManager.Views;
using System.Collections;
using System.ComponentModel;
using System.Windows.Input;

namespace Passwd_VaultManager.ViewModels
{
    internal sealed class EditWindowVM : ViewModelBase {

        private const int MinLength = 21;
        private const int MaxLength = 41;

        private const double BitsPerChar = 6.27; // log2(78) ≈ 6.27 (recompute if alphabet changes)

        private string _appName = String.Empty;
        private string _userName = String.Empty;
        private string _appPasswd = String.Empty;

        private bool _isUserNameSet = false;
        private bool _isPasswdSet = false;

        private int _bitRate = 256;  // secure default
        private int _length = 41;   // reasonable default

        private string _sliderValue = String.Empty;

        public EditWindowVM() {
            
        }

        public EditWindowVM(AppVault appVault) {
            _appName = appVault?.AppName ?? String.Empty;
            _userName = appVault?.UserName ?? String.Empty;
            _appPasswd = appVault?.Password ?? String.Empty;
        }


        // GETTERS AND SETTERS
        public int BitRate {
            get => _bitRate;
            set {
                if (value != 128 && value != 192 && value != 256) {
                    AddError(nameof(BitRate), "Bit rate must be 128, 192, or 256.");
                    return;
                }
                ClearErrors(nameof(BitRate));
                if (_bitRate != value) {
                    _bitRate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RequiredLength));
                    OnPropertyChanged(nameof(PasswdStatus));
                }
            }
        }

        public int Length {
            get => _length;
            set {
                if (value < MinLength || value > MaxLength) {
                    AddError(nameof(Length), $"Length must be between {MinLength} and {MaxLength}.");
                    return;
                }
                ClearErrors(nameof(Length));
                if (_length != value) {
                    _length = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PasswdStatus));
                }
            }
        }

        public string SliderValue {
            get => _sliderValue;
            set {
                if (value == _sliderValue) return;
                _sliderValue = value;
                OnPropertyChanged();
            }
        }

        public string AppName { 
            get => _appName; 
            set { 
                if (_appName == value) return; 
                _appName = value; 
                OnPropertyChanged(); 
            } 
        }        

        public string Username {
            get => _userName; 
            set {
                if (_userName == value) return;
                _userName = value; 
                OnPropertyChanged();
            }
        }

        public string Password {
            get => _appPasswd; 
            set {
                if (_appPasswd == value) return;
                _appPasswd = value;
                OnPropertyChanged();
            }
        }

        public bool UserNameSet {
            get => _isUserNameSet; 
            set {
                if (_isUserNameSet == value) return;
                _isUserNameSet = value;
                OnPropertyChanged();
            }
        }

        public bool PasswdSet {
            get => _isPasswdSet; 
            set { 
                if(_isPasswdSet == value) return;
                _isPasswdSet = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Length needed to meet the selected BitRate with the current alphabet.
        /// </summary>
        public int RequiredLength => (int)Math.Ceiling(BitRate / BitsPerChar);

        /// <summary>
        /// UI-friendly status; never includes the password itself.
        /// </summary>
        public string PasswdStatus => $"{Length} chars : {BitRate}-bit.";

        // ----- INotifyDataErrorInfo (simple impl) -----
        private readonly Dictionary<string, List<string>> _errors = new();

        public bool HasErrors => _errors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
            => propertyName != null && _errors.TryGetValue(propertyName, out var list) ? list : Array.Empty<string>();

        private void AddError(string prop, string message) {
            if (!_errors.TryGetValue(prop, out var list))
                _errors[prop] = list = new List<string>();
            if (!list.Contains(message)) {
                list.Add(message);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
            }
        }

        private void ClearErrors(string prop) {
            if (_errors.Remove(prop))
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
        }
    }
}
