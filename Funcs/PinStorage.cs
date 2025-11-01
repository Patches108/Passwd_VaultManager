using System.IO;
using System.Security.Cryptography;
using System.Text;
using Passwd_VaultManager.Models;

namespace Passwd_VaultManager.Funcs {
    public static class PinStorage {

        // Format stored in file: base64(salt) + ":" + base64(hash)
        public static bool HasPin() {
            AppPaths.EnsureAppDataFolder();
            return File.Exists(AppPaths.PinFile);
        }

        public static void SetPin(string pin) {
            if (string.IsNullOrWhiteSpace(pin) || pin.Length != 4)
                throw new ArgumentException("PIN must be 4 digits.", nameof(pin));

            AppPaths.EnsureAppDataFolder();

            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = HashPin(pin, salt);

            string line = $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
            File.WriteAllText(AppPaths.PinFile, line, Encoding.UTF8);
        }

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

        private static byte[] HashPin(string pin, byte[] salt) {
            // SHA256(salt || pin). For even stronger: use PBKDF2 with iterations.
            using var sha = SHA256.Create();
            byte[] pinBytes = Encoding.UTF8.GetBytes(pin);
            byte[] input = new byte[salt.Length + pinBytes.Length];
            Buffer.BlockCopy(salt, 0, input, 0, salt.Length);
            Buffer.BlockCopy(pinBytes, 0, input, salt.Length, pinBytes.Length);
            return sha.ComputeHash(input);
        }

        public static void RemovePin() {
            AppPaths.EnsureAppDataFolder();
            if (File.Exists(AppPaths.PinFile))
                File.Delete(AppPaths.PinFile);
        }

    }
}
