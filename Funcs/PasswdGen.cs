using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Passwd_VaultManager.Funcs {
    internal class PasswdGen {
        // Ambiguity-reduced sets
        private static readonly char[] Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] Lower = "abcdefghijklmnopqrstuvwxyz".ToCharArray(); // includes 'l' – keep or remove as you prefer
        private static readonly char[] Digits = "23456789".ToCharArray();                  // no 0/1
        private static readonly char[] Symbols = "!@#$%^&*()-_=+[]{};:,.?/|~".ToCharArray();

        private static readonly char[] All = Upper.Concat(Lower).Concat(Digits).Concat(Symbols).ToArray();

        /// <summary>
        /// Generate a cryptographically-strong password that meets (at least) the requested entropy in bits.
        /// </summary>
        /// <param name="bitRate">Target entropy in bits (e.g. 128, 192, 256). If null, defaults to 128.</param>
        public string GenPassword(int? bitRate = null) {
            int targetBits = bitRate ?? 128;
            if (targetBits <= 0) throw new ArgumentOutOfRangeException(nameof(bitRate));

            // Calculate required length from alphabet size
            int alphabetSize = All.Length;                  // with sets above this is 78
            double bitsPerChar = Math.Log(alphabetSize, 2); // ~6.27 for 78
            int requiredLen = (int)Math.Ceiling(targetBits / bitsPerChar);

            // Keep at least 4 so we can place one of each category nicely
            int finalLen = Math.Max(4, requiredLen);

            var pwd = new char[finalLen];
            int i = 0;

            // Ensure at least one of each category when possible
            if (finalLen >= 4) {
                pwd[i++] = Pick(Upper);
                pwd[i++] = Pick(Lower);
                pwd[i++] = Pick(Digits);
                pwd[i++] = Pick(Symbols);
            }

            // Fill the rest uniformly from the full set
            for (; i < finalLen; i++)
                pwd[i] = Pick(All);

            // Remove placement bias
            Shuffle(pwd);

            return new string(pwd);
        }

        private static char Pick(IReadOnlyList<char> set) {
            int idx = RandomNumberGenerator.GetInt32(set.Count);
            return set[idx];
        }

        private static void Shuffle(char[] a) {
            for (int n = a.Length - 1; n > 0; n--) {
                int j = RandomNumberGenerator.GetInt32(n + 1);
                (a[n], a[j]) = (a[j], a[n]);
            }
        }
    }
}
