using System.IO;
using System.Security.Cryptography;
using System.Text;
using Passwd_VaultManager.Models;

namespace Passwd_VaultManager.Funcs {
    public static class PinStorage {

        /// <summary>
        /// Determines whether a PIN file exists in the application data folder.
        /// </summary>
        /// <returns>true if the PIN file exists; otherwise, false.</returns>
        public static bool HasPin() {
            AppPaths.EnsureAppDataFolder();
            return File.Exists(AppPaths.PinFile);
        }

        /// <summary>
        /// Stores a hashed and salted 4-digit PIN in the application data folder.
        /// </summary>
        /// <param name="pin">The 4-digit PIN to set.</param>
        /// <exception cref="ArgumentException">Thrown if the provided PIN is null, empty, or not exactly 4 digits.</exception>
        public static void SetPin(string pin) {
            if (string.IsNullOrWhiteSpace(pin) || pin.Length != 4)
                throw new ArgumentException("PIN must be 4 digits.", nameof(pin));

            AppPaths.EnsureAppDataFolder();

            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = HashPin(pin, salt);

            string line = $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
            File.WriteAllText(AppPaths.PinFile, line, Encoding.UTF8);
        }

        /// <summary>
        /// Verifies the provided PIN against the stored PIN hash.
        /// </summary>
        /// <param name="pin">The PIN code to verify.</param>
        /// <returns>True if the PIN is correct or no PIN is set; otherwise, false.</returns>
        public static bool VerifyPin(string pin) {
            if (!HasPin()) return true; // if no PIN set, allow
            var line = File.ReadAllText(AppPaths.PinFile, Encoding.UTF8);
            var parts = line.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expected = Convert.FromBase64String(parts[1]);
            byte[] actual = HashPin(pin, salt);

            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }

        /// <summary>
        /// Computes a SHA-256 hash of the specified PIN combined with the provided salt.
        /// </summary>
        /// <param name="pin">The PIN value to hash.</param>
        /// <param name="salt">The salt to combine with the PIN before hashing.</param>
        /// <returns>A byte array containing the SHA-256 hash of the salted PIN.</returns>
        private static byte[] HashPin(string pin, byte[] salt) {
            using var sha = SHA256.Create();
            byte[] pinBytes = Encoding.UTF8.GetBytes(pin);
            byte[] input = new byte[salt.Length + pinBytes.Length];
            Buffer.BlockCopy(salt, 0, input, 0, salt.Length);
            Buffer.BlockCopy(pinBytes, 0, input, salt.Length, pinBytes.Length);
            return sha.ComputeHash(input);
        }

        /// <summary>
        /// Deletes the stored PIN file from the application's data folder if it exists.
        /// </summary>
        public static void RemovePin() {
            AppPaths.EnsureAppDataFolder();
            if (File.Exists(AppPaths.PinFile))
                File.Delete(AppPaths.PinFile);
        }

    }
}
