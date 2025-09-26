using Passwd_VaultManager.Models;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Passwd_VaultManager.ViewModels {
    internal sealed class NewWindowVM : ViewModelBase, INotifyDataErrorInfo {
        
        private const int MinLength = 21;
        private const int MaxLength = 41;

        // if you keep your PasswdGen alphabet near ~78 chars:
        private const double BitsPerChar = 6.27; // log2(78) ≈ 6.27 (recompute if alphabet changes)

        private int _bitRate = 128;  // secure default
        private int _length = 21;   // reasonable default

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
