// Password Vault Manager
// Copyright © 2026 Max C (aka Finn).
// All rights reserved.
//
// Licensed under the Password Vault Manager Source-Available License.
// Non-commercial use only.
//
// You may view, use, and modify this source code for personal,
// non-commercial purposes. Redistribution (including modified
// versions and compiled binaries) is permitted only if no fee
// is charged and this copyright notice and license are included.
//
// Commercial use, sale of binaries, or distribution for profit
// requires explicit written permission from the copyright holder.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND.
// See the LICENSE file in the project root for full terms.




using Passwd_VaultManager.Models;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Passwd_VaultManager.Funcs {

    public static class EncryptionService {
        private static readonly string KeyFilePath = Path.Combine(AppPaths.AppDataFolder, "key.dat");

        /// <summary>
        /// Initializes the application by generating and securely storing a new encryption key if one does not already
        /// exist.
        /// </summary>
        public static void Initialize() {
            if (!File.Exists(KeyFilePath)) {
                AppPaths.EnsureAppDataFolder();
                byte[] aesKey = GenerateRandomKey(32); // 256-bit key
                byte[] encryptedKey = ProtectedData.Protect(aesKey, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(KeyFilePath, encryptedKey);
            }
        }

        
        /// <summary>
        /// Encrypts the specified plain text using AES-GCM and returns the result as a byte array containing the nonce,
        /// ciphertext, and authentication tag.
        /// </summary>
        /// <param name="plainText">The plain text string to encrypt.</param>
        /// <returns>A byte array containing the nonce, ciphertext, and authentication tag, or an empty array if the input is
        /// null.</returns>
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


        /// <summary>
        /// Decrypts a byte array containing an AES-GCM encrypted blob and returns the resulting plaintext string.
        /// </summary>
        /// <param name="blob">The encrypted data blob containing the nonce, ciphertext, and authentication tag.</param>
        /// <returns>The decrypted plaintext string, or an empty string if the input is null or empty.</returns>
        /// <exception cref="CryptographicException">Thrown if the blob is invalid or decryption fails.</exception>
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

        /// <summary>
        /// Loads and decrypts the key from the specified key file using the current user's data protection scope.
        /// </summary>
        /// <returns>A byte array containing the decrypted key.</returns>
        private static byte[] LoadKey() {
            byte[] encryptedKey = File.ReadAllBytes(KeyFilePath);
            return ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser);
        }

        /// <summary>
        /// Generates a cryptographically secure random byte array of the specified length.
        /// </summary>
        /// <param name="length">The number of bytes to include in the generated key.</param>
        /// <returns>A byte array containing random values.</returns>
        private static byte[] GenerateRandomKey(int length) {
            byte[] key = new byte[length];
            RandomNumberGenerator.Fill(key);
            return key;
        }
    }
}
