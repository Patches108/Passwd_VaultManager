using Passwd_VaultManager.Models;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Passwd_VaultManager.Funcs {
    public static class EncryptionService {
        private static readonly string KeyFilePath = Path.Combine(AppPaths.AppDataFolder, "key.dat");

        public static void Initialize() {
            if (!File.Exists(KeyFilePath)) {
                AppPaths.EnsureAppDataFolder();
                byte[] aesKey = GenerateRandomKey(32); // 256-bit key
                byte[] encryptedKey = ProtectedData.Protect(aesKey, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(KeyFilePath, encryptedKey);
            }
        }

        // --- GCM helpers that work with BLOBs ---

        public static byte[] EncryptToBlob(string plainText) {
            if (plainText == null) return Array.Empty<byte>();

            byte[] key = LoadKey();
            byte[] nonce = GenerateRandomKey(12); // 96-bit recommended

            byte[] plaintext = Encoding.UTF8.GetBytes(plainText);
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] tag = new byte[16];

            using var gcm = new AesGcm(key);
            gcm.Encrypt(nonce, plaintext, ciphertext, tag);

            // Layout: [nonce | ciphertext | tag]
            byte[] blob = new byte[nonce.Length + ciphertext.Length + tag.Length];
            Buffer.BlockCopy(nonce, 0, blob, 0, nonce.Length);
            Buffer.BlockCopy(ciphertext, 0, blob, nonce.Length, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, blob, nonce.Length + ciphertext.Length, tag.Length);
            return blob;
        }

        public static string DecryptFromBlob(byte[] blob) {
            if (blob == null || blob.Length == 0) return string.Empty;

            byte[] key = LoadKey();

            // Extract parts
            var nonce = new byte[12];
            var tag = new byte[16];
            var ciphertextLen = blob.Length - nonce.Length - tag.Length;
            if (ciphertextLen < 0) throw new CryptographicException("Invalid blob.");

            var ciphertext = new byte[ciphertextLen];
            Buffer.BlockCopy(blob, 0, nonce, 0, nonce.Length);
            Buffer.BlockCopy(blob, nonce.Length, ciphertext, 0, ciphertextLen);
            Buffer.BlockCopy(blob, nonce.Length + ciphertextLen, tag, 0, tag.Length);

            var plaintext = new byte[ciphertextLen];
            using var gcm = new AesGcm(key);
            gcm.Decrypt(nonce, ciphertext, tag, plaintext);
            return Encoding.UTF8.GetString(plaintext);
        }

        private static byte[] LoadKey() {
            byte[] encryptedKey = File.ReadAllBytes(KeyFilePath);
            return ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser);
        }

        private static byte[] GenerateRandomKey(int length) {
            byte[] key = new byte[length];
            RandomNumberGenerator.Fill(key);
            return key;
        }
    }
}
