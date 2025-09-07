using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Passwd_VaultManager.Models {
    public class BoolToImageConverter : IValueConverter {

        public string ImageSource_StatusGood { get; set; } = "pack://application:,,,/Images/Check.png";
        public string ImageSource_StatusBad { get; set; } = "pack://application:,,,/Images/Bad.png";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {           

            string uri = (value is bool b && b) ? ImageSource_StatusGood : ImageSource_StatusBad;
            return new BitmapImage(new Uri(uri, uriKind: UriKind.RelativeOrAbsolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
