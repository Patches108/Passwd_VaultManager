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




using System.IO;

namespace Passwd_VaultManager.Models {
    /// <summary>
    /// Centralized definitions for all application-specific file system paths.
    /// 
    /// This class provides strongly-typed access to directories and files
    /// stored under the user's roaming AppData folder. All paths used by
    /// the application (database, settings, backups, etc.) should be defined
    /// here to avoid hard-coded strings scattered throughout the codebase.
    /// </summary>
    public static class AppPaths {
        /// <summary>
        /// Gets the root application data directory located in the user's
        /// roaming AppData folder.
        /// 
        /// Example:
        /// %APPDATA%\PasswordVaultManager
        /// </summary>
        public static string AppDataFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PasswordVaultManager");

        /// <summary>
        /// Gets the full path to the encrypted vault database file.
        /// </summary>
        public static string DatabaseFile => Path.Combine(AppDataFolder, "Vault.db");

        /// <summary>
        /// Gets the full path to the application's settings configuration file.
        /// </summary>
        public static string SettingsFile => Path.Combine(AppDataFolder, "settings.ini");

        /// <summary>
        /// Gets the directory path used to store automatic and manual
        /// backup copies of the vault database.
        /// </summary>
        public static string BackupFolder => Path.Combine(AppDataFolder, "Backups");

        /// <summary>
        /// Gets the full path to the file used to store the user's PIN data.
        /// </summary>
        public static string PinFile => Path.Combine(AppDataFolder, "pin.dat");

        /// <summary>
        /// Ensures that all required application directories exist.
        /// 
        /// This method creates the application data folder and any required
        /// subdirectories if they do not already exist. It is safe to call
        /// this method multiple times.
        /// </summary>
        public static void EnsureAppDataFolder() {
            Directory.CreateDirectory(AppDataFolder);
            Directory.CreateDirectory(BackupFolder);
        }
    }
}
