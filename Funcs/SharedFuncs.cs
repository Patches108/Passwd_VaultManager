namespace Passwd_VaultManager.Funcs {
    using Passwd_VaultManager.Models;
    using System;
    using System.Buffers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Media;

    public static class SharedFuncs {
        
        /// <summary>
        /// Builds a display string from the given password, excluding specified characters, with optional masking and
        /// length control.
        /// </summary>
        /// <param name="fullPassword">The full password to process.</param>
        /// <param name="excludedChars">Characters to exclude from the display output.</param>
        /// <param name="targetLength">The maximum number of characters to include in the display string; 0 means no limit.</param>
        /// <param name="availableLen">Outputs the number of available characters after exclusions.</param>
        /// <param name="mask">If true, masks the output with bullet characters instead of showing actual characters.</param>
        /// <returns>A string representing the processed password, masked or unmasked, according to the specified parameters.</returns>
        public static string BuildDisplay(
            ReadOnlySpan<char> fullPassword,
            ReadOnlySpan<char> excludedChars,
            int targetLength,
            out int availableLen,
            bool mask = false) {
            var excludeSet = excludedChars.Length > 0
                ? new HashSet<char>(excludedChars.ToArray())
                : null;

            char[] rented = ArrayPool<char>.Shared.Rent(fullPassword.Length);
            int w = 0;

            try {
                for (int i = 0; i < fullPassword.Length; i++) {
                    char c = fullPassword[i];
                    if (excludeSet != null && excludeSet.Contains(c)) continue;
                    rented[w++] = c;
                }

                availableLen = w;

                if (targetLength < 0) targetLength = 0;
                if (targetLength > availableLen) targetLength = availableLen;

                int outLen = targetLength == 0 ? availableLen : targetLength; // choose your “0 means no limit” rule
                if (outLen <= 0) return string.Empty;

                return mask ? new string('•', outLen) : new string(rented, 0, outLen);
            } finally {
                Array.Clear(rented, 0, w);
                ArrayPool<char>.Shared.Return(rented);
            }
        }



        /// <summary>
        /// Trims the input string and validates that it is not null, does not exceed 100 characters, and does not
        /// contain control characters.
        /// </summary>
        /// <param name="value">The string value to validate.</param>
        /// <param name="PropName">The name of the property associated with the value, used in exception messages.</param>
        /// <returns>The trimmed and validated string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the input value is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the input value exceeds 100 characters.</exception>
        /// <exception cref="ArgumentException">Thrown if the input value contains control or escape characters.</exception>
        public static string ValidateString(string value, string PropName) {

            var s = value?.Trim() ?? throw new ArgumentNullException(PropName);

            //if (s.Length == 0) throw new ArgumentException("Value cannot be empty.", PropName);
            if (s.Length > 100) throw new ArgumentOutOfRangeException("Value cannot exceed 100 characters.", PropName);
            if (s.Any(char.IsControl)) throw new ArgumentException("Value cannot contain controls or escape characters - Use numbers, letters and symbols only.", PropName);

            return s;
        }

        /// <summary>
        /// Determines whether the specified integer is between 8 and 256, inclusive.
        /// </summary>
        /// <param name="val">The integer value to validate.</param>
        /// <returns>true if the value is between 8 and 256; otherwise, false.</returns>
        public static bool ValidateNumeral(int val) {
            return val >= 8 && val <= 256;
        }

        /// <summary>
        /// Applies font family and size settings to the visual tree starting at the specified root element.
        /// </summary>
        /// <param name="root">The root DependencyObject to which the font settings will be applied.</param>
        /// <param name="settings">The application settings containing font family and size information.</param>
        public static void Apply(DependencyObject root, AppSettings settings) {
            if (root == null || settings == null) return;

            var family = new FontFamily(settings.FontFamily);
            var size = settings.FontSize;

            ApplyRecursive(root, family, size);
        }

        /// <summary>
        /// Recursively applies the specified font family and size to the given DependencyObject and its visual
        /// children.
        /// </summary>
        /// <remarks>Also handles FlowDocument and RichTextBox content elements that are not traversed by
        /// VisualTreeHelper.</remarks>
        /// <param name="parent">The root DependencyObject to which the font settings are applied.</param>
        /// <param name="family">The FontFamily to apply.</param>
        /// <param name="size">The font size to apply.</param>
        private static void ApplyRecursive(DependencyObject parent, FontFamily family, double size) {

            ApplyToOne(parent, family, size);

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                ApplyRecursive(child, family, size);
            }

            if (parent is FlowDocumentScrollViewer fdsv && fdsv.Document != null)
                ApplyFlowDocument(fdsv.Document, family, size);

            if (parent is RichTextBox rtb && rtb.Document != null)
                ApplyFlowDocument(rtb.Document, family, size);
        }

        /// <summary>
        /// Sets the font family and size on the specified DependencyObject if it is a Control, TextBlock, or
        /// TextElement.
        /// </summary>
        /// <param name="obj">The DependencyObject to apply the font settings to.</param>
        /// <param name="family">The FontFamily to assign.</param>
        /// <param name="size">The font size to assign.</param>
        private static void ApplyToOne(DependencyObject obj, FontFamily family, double size) {
            // Controls (Button, Label, TextBox, ComboBox, etc.)
            if (obj is Control c) {
                c.FontFamily = family;
                c.FontSize = size;
                return;
            }

            // TextBlock (NOT a Control!)
            if (obj is TextBlock tb) {
                tb.FontFamily = family;
                tb.FontSize = size;
                return;
            }

            // TextElement (Run/Span/Paragraph/etc.) – for FlowDocument parts
            if (obj is TextElement te) {
                te.FontFamily = family;
                te.FontSize = size;
                return;
            }
        }


        /// <summary>
        /// Sets the font family and size for a FlowDocument and all its blocks.
        /// </summary>
        /// <param name="doc">The FlowDocument to modify.</param>
        /// <param name="family">The font family to apply.</param>
        /// <param name="size">The font size to apply.</param>
        private static void ApplyFlowDocument(FlowDocument doc, FontFamily family, double size) {
            doc.FontFamily = family;
            doc.FontSize = size;

            foreach (var block in doc.Blocks)
                ApplyBlock(block, family, size);
        }



        /// <summary>
        /// Sets the font family and size for the specified block and its inlines if it is a paragraph.
        /// </summary>
        /// <param name="block">The block to apply font settings to.</param>
        /// <param name="family">The font family to assign.</param>
        /// <param name="size">The font size to assign.</param>
        private static void ApplyBlock(Block block, FontFamily family, double size) {
            block.FontFamily = family;
            block.FontSize = size;

            if (block is Paragraph p) {
                foreach (var inline in p.Inlines)
                    ApplyInline(inline, family, size);
            }
        }



        /// <summary>
        /// Recursively sets the font family and size for the specified inline element and all its child inlines.
        /// </summary>
        /// <param name="inline">The inline element to apply the font settings to.</param>
        /// <param name="family">The font family to assign.</param>
        /// <param name="size">The font size to assign.</param>
        private static void ApplyInline(Inline inline, FontFamily family, double size) {
            inline.FontFamily = family;
            inline.FontSize = size;

            if (inline is Span span) {
                foreach (var child in span.Inlines)
                    ApplyInline(child, family, size);
            }
        }

        private static bool IsPropertyBound(DependencyObject obj, DependencyProperty prop) {
            return BindingOperations.IsDataBound(obj, prop);
        }
    }

}
