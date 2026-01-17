using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Passwd_VaultManager.Models {

    /// <summary>
    /// Converts a boolean value to an image source, returning a specified image for true or false values.
    /// </summary>
    public class BoolToImageConverter : IValueConverter {

        public string ImageSource_StatusGood { get; set; } = "pack://application:,,,/Images/Check.png";
        public string ImageSource_StatusBad { get; set; } = "pack://application:,,,/Images/Bad.png";



        /// <summary>
        /// Converts a boolean value into a corresponding status image.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {           

            string uri = (value is bool b && b) ? ImageSource_StatusGood : ImageSource_StatusBad;
            return new BitmapImage(new Uri(uri, uriKind: UriKind.RelativeOrAbsolute));
        }


        /// <summary>
        /// ConvertBack is not supported for this converter.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
