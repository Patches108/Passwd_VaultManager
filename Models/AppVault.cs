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


using System.ComponentModel;
using System.Runtime.CompilerServices;
using Passwd_VaultManager.Funcs;

namespace Passwd_VaultManager.Models {

    /// <summary>
    /// Represents an application or account vault entry, storing metadata, credentials, and related status flags with
    /// property change notification support.
    /// </summary>
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



        /// <summary>
        /// Initializes a new instance of the AppVault class and assigns a unique identifier to InstanceGuid.
        /// </summary>
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
        /// <summary>
        /// Validates and sets a string field, raising a property change notification if the value changes.
        /// </summary>
        /// <param name="field">Reference to the backing string field to update.</param>
        /// <param name="value">The new string value to validate and assign.</param>
        /// <param name="propName">The name of the property associated with the field.</param>
        private void SetValidatedString(ref string field, string? value, [CallerMemberName] string? propName = null) {
            value ??= string.Empty;
            if (field == value) return;

            // Keep model pure: validate or throw, don't show dialogs here.
            field = SharedFuncs.ValidateString(value, propName ?? "Value");
            OnPropertyChanged(propName);
        }


        /// <summary>
        /// Sets the specified field to a new value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the field and value.</typeparam>
        /// <param name="field">Reference to the field to be updated.</param>
        /// <param name="value">The new value to assign to the field.</param>
        /// <param name="propName">The name of the property that changed.</param>
        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propName = null) {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propName);
        }



        /// <summary>
        /// Raises the PropertyChanged event to notify listeners of a property value change.
        /// </summary>
        /// <param name="name">The name of the property that changed.</param>
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
