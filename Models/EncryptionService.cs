using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace Passwd_VaultManager.Models
{
    public static class EncryptionService
    {
        private static readonly string KeyFilePath = Path.Combine(AppPaths.AppDataFolder, "key.dat");

        /// <summary>
        /// Ensure encryption key exists. Call this on app startup.
        /// </summary>
        public static void Initialize() {
            if (!File.Exists(KeyFilePath)) {
                AppPaths.EnsureAppDataFolder(); // Ensures AppData and BackupFolder exist
                byte[] aesKey = GenerateRandomKey(32); // AES-256
                byte[] encryptedKey = ProtectedData.Protect(aesKey, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(KeyFilePath, encryptedKey);
            }
        }

        /// <summary>
        /// Encrypts a plaintext string using AES with a DPAPI-protected key.
        /// </summary>
        public static string Encrypt(string plainText) {
            if (string.IsNullOrEmpty(plainText)) return null;

            byte[] key = LoadKey();
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Prepend IV to cipher text
            byte[] combined = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(combined);
        }

        /// <summary>
        /// Decrypts an AES-encrypted string using the DPAPI-protected key.
        /// </summary>
        public static string Decrypt(string cipherText) {
            if (string.IsNullOrEmpty(cipherText)) return null;

            byte[] fullCipher = Convert.FromBase64String(cipherText);
            byte[] key = LoadKey();

            using Aes aes = Aes.Create();
            aes.Key = key;

            // Extract IV and cipher text
            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] cipherBytes = new byte[fullCipher.Length - iv.Length];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

            aes.IV = iv;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        private static byte[] LoadKey() {
            byte[] encryptedKey = File.ReadAllBytes(KeyFilePath);
            return ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser);
        }

        private static byte[] GenerateRandomKey(int length) {
            byte[] key = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return key;
        }
    }
}
