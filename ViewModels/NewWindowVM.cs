using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Views;

namespace Passwd_VaultManager.ViewModels {
    internal sealed class NewWindowVM : ViewModelBase {
        private const int MinLength = 8;
        private const int MaxLength = 41;

        // Must match your generator alphabet (78 chars => ~6.285 bits/char)
        private const double BitsPerChar = 6.285;
        private const int BitsCap = 256;

        private int _length = 21;              // actual displayed/saved length
        private int _bitRate = 128;            // derived from length by default

        private int _targetLength = 21;        // slider value: requested output length
        private int _sliderMaxLength = 41;     // slider maximum: available length after exclusions

        // --- Slider state (NEW) ---

        /// <summary>Slider Value: requested output length.</summary>
        public int TargetLength {
            get => _targetLength;
            set {
                int clamped = Math.Clamp(value, MinLength, SliderMaxLength);
                if (_targetLength == clamped) return;
                _targetLength = clamped;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SliderValueText));
            }
        }

        /// <summary>Slider Maximum: available length after exclusions (clamped to [MinLength..MaxLength]).</summary>
        public int SliderMaxLength {
            get => _sliderMaxLength;
            set {
                int clamped = Math.Clamp(value, MinLength, MaxLength);
                if (_sliderMaxLength == clamped) return;

                _sliderMaxLength = clamped;
                OnPropertyChanged();

                // if max shrinks, ensure target stays valid
                if (_targetLength > _sliderMaxLength)
                    TargetLength = _sliderMaxLength;

                OnPropertyChanged(nameof(SliderValueText));
            }
        }

        /// <summary>If you want to bind a label to show the slider value as text.</summary>
        public string SliderValueText => TargetLength.ToString();

        // --- Outputs ---

        /// <summary>Actual current password length (displayed/saved).</summary>
        public int Length {
            get => _length;
            set {
                int clamped = Math.Clamp(value, MinLength, MaxLength);
                if (_length == clamped) return;

                _length = clamped;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PasswdStatus));

                // keep bitrate consistent with length
                //BitRate = CalculateBits(_length);
                BitRate = Math.Min((int)Math.Ceiling(_length * BitsPerChar), BitsCap);
            }
        }

        /// <summary>Entropy estimate derived from Length.</summary>
        public int BitRate {
            get => _bitRate;
            private set {
                int clamped = Math.Clamp(value, 0, BitsCap);
                if (_bitRate == clamped) return;

                _bitRate = clamped;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PasswdStatus));
            }
        }

        public string PasswdStatus {
            get {
                int bRate = Math.Min((int)Math.Ceiling(Length * BitsPerChar), BitsCap);
                return $"{Length} chars : {bRate}-bit.";
            }
        }

        //private static int CalculateBits(int length) {
        //    int bits = (int)Math.Ceiling(length * BitsPerChar);
        //    return Math.Min(bits, BitsCap);

        //}
    }
}
