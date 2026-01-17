using System.Globalization;
using System.Windows.Data;

namespace Passwd_VaultManager.Models {
    /// <summary>
    /// Converts a boolean value into a human-readable text representation.
    /// 
    /// This converter is typically used in WPF bindings to display
    /// user-friendly status text (e.g. "Set" / "Not Set") based on a
    /// boolean property.
    /// </summary>
    public class BoolToTextConverter : IValueConverter {

        public string TrueText { get; set; } = "Set";
        public string FalseText { get; set; } = "Not Set";


        /// <summary>
        /// Converts a boolean value to its corresponding text representation.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">Optional converter parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// <see cref="TrueText"/> if the value is <c>true</c>;
        /// otherwise <see cref="FalseText"/>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            if (value is bool b)
                return b ? TrueText : FalseText;
            else    
                return FalseText;
        }



        /// <summary>
        /// Conversion back from text to boolean is not supported.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The type of the binding source property.</param>
        /// <param name="parameter">Optional converter parameter (not used).</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Always throws a <see cref="NotImplementedException"/>.</returns>
        /// <exception cref="NotImplementedException">
        /// This converter supports one-way binding only.
        /// </exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
