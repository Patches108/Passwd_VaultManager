// Password Vault Manager
// Copyright © 2026 Max C (aka Finn).
// All rights reserved.
//
// Licensed under the Password Vault Manager Source-Available License.
// Non-commercial use only.
//
// You may view, use, and modify this source code for personal,
// non-commercial purposes. Redistribution (including modified
// versions and compiled binaries) is permitted only if no fee
// is charged and this copyright notice and license are included.
//
// Commercial use, sale of binaries, or distribution for profit
// requires explicit written permission from the copyright holder.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND.
// See the LICENSE file in the project root for full terms.


using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Views;

namespace Passwd_VaultManager.ViewModels {


    /// <summary>
    /// ViewModel for the Edit window.
    /// 
    /// Provides bindable fields for editing an existing <see cref="AppVault"/> entry
    /// (app name, username, password, excluded characters) and exposes slider-related
    /// properties for password length/strength display. Changes are validated and
    /// persisted back to the underlying vault record.
    /// </summary>
    internal sealed class EditWindowVM : ViewModelBase {
        private const int MinLength = 8;
        private const int MaxLength = 41;

        private const double BitsPerChar = 6.285;
        private const int BitsCap = 256;

        private string _appName = string.Empty;
        private string _userName = string.Empty;
        private string _password = string.Empty;        // what you save (displayed/revealed when saving)
        private string _excludedChars = string.Empty;

        private int _length = 41;                       // actual displayed/saved length
        private int _targetLength = 41;                 // slider value (requested length cap)
        private int _sliderMaxLength = 41;              // available length after exclusions

        private int _bitRate = 256;

        private readonly AppVault _backingVault;

        public EditWindowVM() { }


       
        public EditWindowVM(AppVault appVault) {
            _backingVault = appVault;

            _appName = appVault?.AppName ?? string.Empty;
            _userName = appVault?.UserName ?? string.Empty;
            _password = appVault?.Password ?? string.Empty;
            _excludedChars = appVault?.ExcludedChars ?? string.Empty;

            _bitRate = appVault?.BitRate ?? 256;

            // Seed lengths
            _length = Math.Clamp(_password?.Length ?? 0, MinLength, MaxLength);
            _targetLength = _length;
            _sliderMaxLength = _length;
        }

        // --- Core fields ---
        public string AppName {
            get => _appName;
            set {
                if (_appName == value) return;
                try {
                    _appName = SharedFuncs.ValidateString(value, nameof(value));
                    OnPropertyChanged();
                } catch (Exception ex) {
                    new MessageWindow($"ERROR:\n\n{ex.Message}", SoundController.ErrorSound).ShowDialog();
                }
            }
        }

        public string Username {
            get => _userName;
            set {
                if (_userName == value) return;
                try {
                    _userName = SharedFuncs.ValidateString(value, nameof(value));
                    OnPropertyChanged();
                } catch (Exception ex) {
                    new MessageWindow($"ERROR:\n\n{ex.Message}", SoundController.ErrorSound).ShowDialog();
                }
            }
        }

        public string Password {
            get => _password;
            set {
                if (_password == value) return;
                try {
                    _password = SharedFuncs.ValidateString(value, nameof(value));
                    OnPropertyChanged();
                } catch (Exception ex) {
                    new MessageWindow($"ERROR:\n\n{ex.Message}", SoundController.ErrorSound).ShowDialog();
                }
            }
        }

        public string ExcludedChars {
            get => _excludedChars;
            set {
                value ??= string.Empty;
                if (_excludedChars == value) return;
                _excludedChars = value;
                OnPropertyChanged();
            }
        }

        // --- Slider plumbing ---

        /// <summary>Slider Value: the requested output length cap.</summary>
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

        /// <summary>Slider Maximum: the available length after exclusions.</summary>
        public int SliderMaxLength {
            get => _sliderMaxLength;
            set {
                int clamped = Math.Clamp(value, MinLength, MaxLength);
                if (_sliderMaxLength == clamped) return;
                _sliderMaxLength = clamped;
                OnPropertyChanged();

                // If max shrinks, force target to stay valid
                if (_targetLength > _sliderMaxLength)
                    TargetLength = _sliderMaxLength;

                OnPropertyChanged(nameof(SliderValueText));
            }
        }

        /// <summary>If you still want a string for labels.</summary>
        public string SliderValueText => TargetLength.ToString();

        // --- Derived outputs ---

        /// <summary>Actual displayed/saved length.</summary>
        public int Length {
            get => _length;
            set {
                int clamped = Math.Clamp(value, MinLength, MaxLength);
                if (_length == clamped) return;
                _length = clamped;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PasswdStatus));

                // keep bitrate consistent with length if that's your rule
                BitRate = Math.Min((int)Math.Ceiling(_length * BitsPerChar), BitsCap);
            }
        }

        public int BitRate {
            get => _bitRate;
            set {
                if (_bitRate == value) return;
                _bitRate = Math.Clamp(value, 0, BitsCap);
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


        /// <summary>
        /// Persists the current ViewModel values back to the underlying vault record
        /// and updates the database.
        /// 
        /// This also refreshes derived status flags (e.g., whether required fields are set)
        /// and stores the calculated bitrate/strength value.
        /// </summary>
        /// <returns>A task that completes when the database update finishes.</returns>
        public async Task SaveAsync() {
            _backingVault.AppName = AppName;
            _backingVault.UserName = Username;
            _backingVault.Password = Password;
            _backingVault.ExcludedChars = ExcludedChars;

            _backingVault.IsUserNameSet = !string.IsNullOrWhiteSpace(Username);
            _backingVault.IsPasswdSet = !string.IsNullOrWhiteSpace(Password);
            _backingVault.IsAppNameSet = !string.IsNullOrWhiteSpace(AppName) && !AppName.Equals("No App/Account Name");

            _backingVault.IsStatusGood = _backingVault.IsUserNameSet && _backingVault.IsPasswdSet && _backingVault.IsAppNameSet;
            _backingVault.BitRate = BitRate;

            await DatabaseHandler.UpdateVaultAsync(_backingVault);
        }
    }
}
