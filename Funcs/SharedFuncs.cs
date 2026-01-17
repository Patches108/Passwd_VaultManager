namespace Passwd_VaultManager.Funcs {
    using Passwd_VaultManager.Models;
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Media;

    public static class SharedFuncs {
        /// <summary>
        /// Builds display text from a password source while minimizing long-lived plaintext.
        /// If mask is true, returns bullets instead of the real characters.
        /// </summary>
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
        /// Validates string input, throws exception is invalid.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="PropName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string ValidateString(string value, string PropName) {

            var s = value?.Trim() ?? throw new ArgumentNullException(PropName);

            //if (s.Length == 0) throw new ArgumentException("Value cannot be empty.", PropName);
            if (s.Length > 100) throw new ArgumentOutOfRangeException("Value cannot exceed 100 characters.", PropName);
            if (s.Any(char.IsControl)) throw new ArgumentException("Value cannot contain controls or escape characters - Use numbers, letters and symbols only.", PropName);

            return s;
        }

        public static bool ValidateNumeral(int val) {
            return val >= 8 && val <= 256;
        }

        /// <summary>
        /// Applies font settings recursively to all controls in a window.
        /// </summary>
        public static void Apply(DependencyObject root, AppSettings settings) {
            if (root == null || settings == null) return;

            var family = new FontFamily(settings.FontFamily);
            var size = settings.FontSize;

            ApplyRecursive(root, family, size);
        }

        private static void ApplyRecursive(DependencyObject parent, FontFamily family, double size) {
            // 1) Apply to the parent itself (important for usercontrols too)
            ApplyToOne(parent, family, size);

            // 2) Apply to visual children
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++) {
                var child = VisualTreeHelper.GetChild(parent, i);
                ApplyRecursive(child, family, size);
            }

            // 3) Also handle ContentElements (FlowDocument etc.)
            // VisualTreeHelper doesn't walk these, so we handle common cases:
            if (parent is FlowDocumentScrollViewer fdsv && fdsv.Document != null)
                ApplyFlowDocument(fdsv.Document, family, size);

            if (parent is RichTextBox rtb && rtb.Document != null)
                ApplyFlowDocument(rtb.Document, family, size);
        }

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

        private static void ApplyFlowDocument(FlowDocument doc, FontFamily family, double size) {
            doc.FontFamily = family;
            doc.FontSize = size;

            foreach (var block in doc.Blocks)
                ApplyBlock(block, family, size);
        }

        private static void ApplyBlock(Block block, FontFamily family, double size) {
            block.FontFamily = family;
            block.FontSize = size;

            if (block is Paragraph p) {
                foreach (var inline in p.Inlines)
                    ApplyInline(inline, family, size);
            }
        }

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
