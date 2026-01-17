using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Passwd_VaultManager.Funcs {
    internal class PasswdGen {
        // Ambiguity-reduced sets
        private static readonly char[] Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] Lower = "abcdefghijklmnopqrstuvwxyz".ToCharArray(); 
        private static readonly char[] Digits = "23456789".ToCharArray();                  // no 0/1
        private static readonly char[] Symbols = "!@#$%^&*()-_=+[]{};:,.?/|~".ToCharArray();

        private static readonly char[] All = Upper.Concat(Lower).Concat(Digits).Concat(Symbols).ToArray();

        /// <summary>
        /// Generates a random password containing at least one uppercase letter, one lowercase letter, one digit, and
        /// one symbol, with entropy based on the specified bit rate.
        /// </summary>
        /// <param name="bitRate">The desired entropy in bits for the generated password. Defaults to 128 if not specified.</param>
        /// <returns>A randomly generated password string meeting the specified entropy and character requirements.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified bitRate is less than or equal to zero.</exception>
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

            for (; i < finalLen; i++)
                pwd[i] = Pick(All);

            Shuffle(pwd);

            return new string(pwd);
        }

        /// <summary>
        /// Selects a random character from the specified set.
        /// </summary>
        /// <param name="set">The collection of characters to choose from.</param>
        /// <returns>A randomly selected character from the set.</returns>
        private static char Pick(IReadOnlyList<char> set) {
            int idx = RandomNumberGenerator.GetInt32(set.Count);
            return set[idx];
        }

        /// <summary>
        /// Randomly shuffles the elements of the specified character array in place.
        /// </summary>
        /// <param name="a">The array of characters to shuffle.</param>
        private static void Shuffle(char[] a) {
            for (int n = a.Length - 1; n > 0; n--) {
                int j = RandomNumberGenerator.GetInt32(n + 1);
                (a[n], a[j]) = (a[j], a[n]);
            }
        }
    }
}
