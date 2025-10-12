namespace Passwd_VaultManager.Funcs {
    using System;
    using System.Buffers;

    public static class SharedFuncs {
        /// <summary>
        /// Builds display text from a password source while minimizing long-lived plaintext.
        /// If mask is true, returns bullets instead of the real characters.
        /// </summary>
        public static string BuildDisplay(
            ReadOnlySpan<char> fullPassword,
            ReadOnlySpan<char> excludedChars,
            int targetLength,
            ReadOnlySpan<char> currentText,
            bool force = false,
            ReadOnlySpan<char> placeholder = default,
            bool mask = false) {
            // 0) Placeholder guard (avoid work and allocations)
            //if (!force && !placeholder.IsEmpty && currentText.SequenceEqual(placeholder))
            //    return new string(currentText); // small, unavoidable string; better to just pass through binding

            // 1) Normalize target length
            if (targetLength < 0) targetLength = 0;
            if (targetLength > 41) targetLength = 41;

            // 2) Early outs
            if (fullPassword.IsEmpty)
                return string.Empty;

            // 3) Build exclusion set (ASCII fast path; extend as needed)
            // For non-ASCII exclusion, consider Rune enumeration + HashSet<int>.
            var excludeSet = excludedChars.Length > 0
                ? new HashSet<char>(excludedChars.ToArray()) // short-lived; OK
                : null;

            // 4) Allocate a pooled buffer for filtered result (worst-case = source length)
            char[] rented = ArrayPool<char>.Shared.Rent(fullPassword.Length);
            int w = 0;

            try {
                // Filter step
                for (int i = 0; i < fullPassword.Length; i++) {
                    char c = fullPassword[i];
                    if (excludeSet != null && excludeSet.Contains(c)) continue;
                    rented[w++] = c;
                }

                // Length step
                int outLen = (targetLength > 0 && w > targetLength) ? targetLength : w;

                if (outLen <= 0) return string.Empty;

                if (mask) {
                    // Return masked view with bullets
                    return new string('•', outLen);
                }

                // Materialize final string (single unavoidable allocation here)
                return new string(rented, 0, outLen);
            } finally {
                // Zero and return buffer to pool
                Array.Clear(rented, 0, w);
                ArrayPool<char>.Shared.Return(rented);
            }
        }
    }

}
