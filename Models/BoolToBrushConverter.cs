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


using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Passwd_VaultManager.Models {

    /// <summary>
    /// Converts boolean values to Brush objects for UI representation, mapping true and false to configurable brushes.
    /// </summary>
    public class BoolToBrushConverter : IValueConverter {
        public Brush TrueBrush { get; set; } = Brushes.LimeGreen;
        public Brush FalseBrush { get; set; } = Brushes.IndianRed;

        

        /// <summary>
        /// Returns TrueBrush if the input value is a boolean and true; otherwise, returns FalseBrush.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target type of the conversion.</param>
        /// <param name="parameter">An optional parameter for the conversion.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>TrueBrush if value is true; otherwise, FalseBrush.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool b)
                return b ? TrueBrush : FalseBrush;

            return FalseBrush;
        }



        /// <summary>
        /// Throws a NotSupportedException to indicate that backward conversion is not supported.
        /// </summary>
        /// <param name="value">The value produced by the binding target to be converted.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>This method does not return a value.</returns>
        /// <exception cref="NotSupportedException">Always thrown to indicate that backward conversion is not supported.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
