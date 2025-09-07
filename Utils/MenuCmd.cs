using System.Windows;
using System.Windows.Controls;

namespace Passwd_VaultManager.Utils
{
    public class MenuCmd : RadioButton
    {
        static MenuCmd() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuCmd), new FrameworkPropertyMetadata(typeof(MenuCmd)));
        }
    }
}
