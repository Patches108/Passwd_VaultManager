using System.Globalization;
using System.Windows.Data;

namespace Passwd_VaultManager.Models
{
    public class BoolToTextConverter : IValueConverter {

        public string TrueText { get; set; } = "Set";
        public string FalseText { get; set; } = "Not Set";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            if (value is bool b)
                return b ? TrueText : FalseText;
            else    
                return FalseText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
