using Passwd_VaultManager.Models;
using Passwd_VaultManager.Views;
using System.Collections;
using System.ComponentModel;

namespace Passwd_VaultManager.ViewModels {
    internal sealed class NewWindowVM : ViewModelBase {

        private const int MinLength = 8;
        private const int MaxLength = 41;

        private const double BitsPerChar = 6.27; // log2(78) ≈ 6.27 (recompute if alphabet changes)

        private int _bitRate = 128;  // secure default
        private int _length = 21;   // reasonable default

        private string _sliderValue = String.Empty;

        public string SliderValue { get => _sliderValue;
            set {
                if (value == _sliderValue) return;
                _sliderValue = value;
                OnPropertyChanged();
            }
        }

        public int BitRate {
            get => _bitRate;
            set {
                if (_bitRate == value) return;

                if (Funcs.SharedFuncs.ValidateNumeral(value)) {
                    _bitRate = value;
                    OnPropertyChanged();
                } else {
                    new MessageWindow("ERROR: Bit Rate must be between 8 and 256");
                }
                //if (value != 128 && value != 192 && value != 256) {
                //    AddError(nameof(BitRate), "Bit rate must be 128, 192, or 256.");
                //    return;
                //}
                //ClearErrors(nameof(BitRate));
                //if (_bitRate != value) {
                //    _bitRate = value;
                //    OnPropertyChanged();
                //    OnPropertyChanged(nameof(RequiredLength));
                //    OnPropertyChanged(nameof(PasswdStatus));
                //}
            }
        }

        public int Length {
            get => _length;
            set {
                if (value < MinLength || value > MaxLength) {
                    new MessageWindow($"Length must be between {MinLength} and {MaxLength}.").Show();
                    return;
                }
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
    }
}
