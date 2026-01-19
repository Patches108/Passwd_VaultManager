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


using System.Windows;
using System.Windows.Controls;

namespace Passwd_VaultManager.Utils
{
    /// <summary>
    /// Custom radio button control used for menu-style commands.
    /// 
    /// This control applies a custom default style defined in XAML,
    /// allowing menu commands to be styled and grouped consistently
    /// without modifying standard <see cref="RadioButton"/> behavior.
    /// </summary>
    public class MenuCmd : RadioButton
    {

        /// <summary>
        /// Overrides the default style key to associate this control
        /// with its custom XAML-defined style.
        /// </summary>
        static MenuCmd() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuCmd), new FrameworkPropertyMetadata(typeof(MenuCmd)));
        }
    }
}
