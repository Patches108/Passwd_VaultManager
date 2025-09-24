using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Passwd_VaultManager.Funcs {
    internal class PasswdGen {
        // Ambiguity-reduced sets (omit 0/O and 1/l/I to help users)
        private static readonly char[] Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] Lower = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly char[] Digits = "23456789".ToCharArray();                  // no 0/1
        private static readonly char[] Symbols = "!@#$%^&*()-_=+[]{};:,.?/|~".ToCharArray();

        private static readonly char[] All = Upper.Concat(Lower).Concat(Digits).Concat(Symbols).ToArray();

        public PasswdGen() { }

        /// <summary>
        /// Generate a cryptographically-strong password. The final length will be the
        /// greater of 'len' and the minimum needed to satisfy 'bitRate' bits of entropy
        /// given the chosen alphabet.
        /// </summary>
        /// <param name="bitRate">Target entropy in bits (e.g., 80, 128, 192, 256).</param>
        /// <param name="len">Preferred minimum length.</param>
        public string GenPassword(int bitRate, int len) {
            if (bitRate <= 0) throw new ArgumentOutOfRangeException(nameof(bitRate));
            if (len <= 0) throw new ArgumentOutOfRangeException(nameof(len));

            int alphabetSize = All.Length;                             // ~ (24+23+8+…)
            double bitsPerChar = Math.Log(alphabetSize, 2.0);
            int minLenForBits = (int)Math.Ceiling(bitRate / bitsPerChar);
            int finalLen = Math.Max(len, minLenForBits);

            var pwd = new char[finalLen];
            int i = 0;

            // Ensure at least one of each category when possible
            if (finalLen >= 4) {
                pwd[i++] = Pick(Upper);
                pwd[i++] = Pick(Lower);
                pwd[i++] = Pick(Digits);
                pwd[i++] = Pick(Symbols);
            }

            // Fill the rest from the full set
            for (; i < finalLen; i++)
                pwd[i] = Pick(All);

            // Shuffle to remove category position bias
            Shuffle(pwd);

            return new string(pwd);
        }

        private static char Pick(IReadOnlyList<char> set) {
            int idx = RandomNumberGenerator.GetInt32(set.Count); // uniform, cryptographic
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
