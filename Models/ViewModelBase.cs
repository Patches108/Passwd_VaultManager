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

namespace Passwd_VaultManager.Models {

    /// <summary>
    /// Base class for ViewModels that provides property change
    /// notification support for data binding.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged {



        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;



        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified
        /// property name.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that changed. This value is supplied
        /// automatically by the compiler when omitted.
        /// </param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
