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
