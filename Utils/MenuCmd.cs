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
