using Microsoft.Data.Sqlite;
using Passwd_VaultManager.Models;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Passwd_VaultManager.Funcs {
    public static class DatabaseHandler {

        private static readonly string _connectionString =
            $"Data Source={System.IO.Path.Combine(AppPaths.AppDataFolder, "Vault.db")};";

        private static readonly string _dbLocation = AppPaths.AppDataFolder + "\\Vault.db";

        public static void initDatabase() {
            if(!File.Exists(_dbLocation)) {
                SQLiteConnection.CreateFile(@""+ _dbLocation);

                using (var conn = new SQLiteConnection(_connectionString)) { 
                    conn.Open();

                    string CreateStandardDatabaseQuery = @"
                        CREATE TABLE IF NOT EXISTS Vault (
                            id             INTEGER PRIMARY KEY AUTOINCREMENT,
                            DateCreated    TEXT    NOT NULL,   
                            AppName        BLOB    NOT NULL,   
                            UserName       BLOB    NOT NULL,   
                            Passwd         BLOB    NOT NULL,   
                            IsUserNameSet  INTEGER NOT NULL,   
                            IsPasswdSet    INTEGER NOT NULL    
                        );
                        ";

                    using (var comm = new SQLiteCommand(conn)) { 
                        comm.CommandText = CreateStandardDatabaseQuery;
                        comm.ExecuteNonQuery();
                    }
                }

                //MessageBox.Show("DB created!");
            }
        }

        public static async Task<long> WriteRecordToDatabaseAsync(AppVault v, CancellationToken ct = default) {

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(ct);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Vault (DateCreated, AppName, UserName, Passwd, IsUserNameSet, IsPasswdSet)
                VALUES ($date, $app, $user, $pwd, $userSet, $pwdSet);
                SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("$date", DateTime.UtcNow.ToString("o"));

            var pApp = cmd.CreateParameter(); pApp.ParameterName = "$app"; pApp.SqliteType = SqliteType.Blob; pApp.Value = EncryptionService.EncryptToBlob(v.AppName); cmd.Parameters.Add(pApp);
            var pUser = cmd.CreateParameter(); pUser.ParameterName = "$user"; pUser.SqliteType = SqliteType.Blob; pUser.Value = EncryptionService.EncryptToBlob(v.UserName ?? ""); cmd.Parameters.Add(pUser);
            var pPwd = cmd.CreateParameter(); pPwd.ParameterName = "$pwd"; pPwd.SqliteType = SqliteType.Blob; pPwd.Value = EncryptionService.EncryptToBlob(v.Password ?? ""); cmd.Parameters.Add(pPwd);

            cmd.Parameters.AddWithValue("$userSet", v.IsUserNameSet ? 1 : 0);
            cmd.Parameters.AddWithValue("$pwdSet", v.IsPasswdSet ? 1 : 0);

            var scalar = await cmd.ExecuteScalarAsync(ct);
            return (scalar is long id) ? id : Convert.ToInt64(scalar);
        }

        // Decrypt when you need to display/use the password
        public static string UnprotectPassword(byte[] blob) {
            byte[] entropy = GetAppEntropy();
            byte[] clear = ProtectedData.Unprotect(blob, entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(clear);
        }

        // Keep this value outside of source control or derive it from machine/user secrets
        private static byte[] GetAppEntropy() =>
            Encoding.UTF8.GetBytes("your-app-entropy-change-this");

        public static void ReadRecordFromDatabase() {

        }

        public static void SearchDBByID(int id) {

        }

        public static async Task<ObservableCollection<AppVault>> GetVaults(CancellationToken ct = default) {
            var vaults = new ObservableCollection<AppVault>();

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(ct);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    id,
                    DateCreated,
                    AppName,        
                    UserName,      
                    Passwd,         
                    IsUserNameSet,  
                    IsPasswdSet     
                FROM Vault;";

            await using var reader = await cmd.ExecuteReaderAsync(ct);

            // Resolve ordinals once
            int ordId = reader.GetOrdinal("id");
            int ordDate = reader.GetOrdinal("DateCreated");
            int ordAppName = reader.GetOrdinal("AppName");
            int ordUserName = reader.GetOrdinal("UserName");
            int ordPasswd = reader.GetOrdinal("Passwd");
            int ordIsUserSet = reader.GetOrdinal("IsUserNameSet");
            int ordIsPassSet = reader.GetOrdinal("IsPasswdSet");

            while (await reader.ReadAsync(ct)) {
                // Decrypt BLOBs (handles empty/null defensively)
                string appName = DecryptBlobSafe(reader, ordAppName);
                string userName = DecryptBlobSafe(reader, ordUserName);
                string password = DecryptBlobSafe(reader, ordPasswd);

                // Booleans: prefer INTEGER 0/1; also tolerate legacy TEXT columns
                bool isUserSet = ReadBoolFlexible(reader, ordIsUserSet);
                bool isPassSet = ReadBoolFlexible(reader, ordIsPassSet);

                // DateCreated: keep as string or parse to DateTime if you like
                string dateCreatedStr = reader.IsDBNull(ordDate) ? "" : reader.GetString(ordDate);
                DateTime? dateCreated = DateTime.TryParse(dateCreatedStr, out var dt) ? dt : null;

                var app = new AppVault {
                    AppName = appName,
                    UserName = userName,
                    Password = password,
                    IsUserNameSet = isUserSet,
                    IsPasswdSet = isPassSet,
                    IsStatusGood = isUserSet && isPassSet // or whatever your rule is
                                                          // you can add a DateCreated property to AppVault if needed
                };

                vaults.Add(app);
            }

            return vaults;
        }


        private static string DecryptBlobSafe(SqliteDataReader reader, int ordinal) {
            if (reader.IsDBNull(ordinal)) return string.Empty;

            // SQLite provider returns BLOB as byte[]
            if (reader.GetFieldType(ordinal) == typeof(byte[])) {
                var blob = (byte[])reader.GetValue(ordinal);
                if (blob.Length == 0) return string.Empty;
                return EncryptionService.DecryptFromBlob(blob);
            }

            // If you still have legacy TEXT base64 storage:
            if (reader.GetFieldType(ordinal) == typeof(string)) {
                var s = reader.GetString(ordinal);
                if (string.IsNullOrEmpty(s)) return string.Empty;
                return s; // If column wasn't encrypted yet
            }

            // Fallback: try get as bytes
            var obj = reader.GetValue(ordinal);
            if (obj is byte[] b && b.Length > 0) return EncryptionService.DecryptFromBlob(b);
            return Convert.ToString(obj) ?? string.Empty;
        }

        private static bool ReadBoolFlexible(SqliteDataReader reader, int ordinal) {
            if (reader.IsDBNull(ordinal)) return false;

            var t = reader.GetFieldType(ordinal);

            if (t == typeof(long) || t == typeof(int)) {
                // Microsoft.Data.Sqlite maps INTEGER to Int64 (long)
                long v = reader.GetInt64(ordinal);
                return v != 0;
            }

            if (t == typeof(string)) {
                var s = reader.GetString(ordinal);
                if (string.IsNullOrWhiteSpace(s)) return false;
                if (bool.TryParse(s, out var b)) return b;
                if (long.TryParse(s, out var n)) return n != 0;
                return string.Equals(s, "y", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(s, "yes", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);
            }

            if (t == typeof(bool))
                return reader.GetBoolean(ordinal);

            // Fallback: attempt numeric truthiness
            var obj = reader.GetValue(ordinal);
            if (obj is long l) return l != 0;
            if (obj is int i) return i != 0;
            if (obj is bool b2) return b2;
            if (obj is string s2) return s2.Equals("true", StringComparison.OrdinalIgnoreCase) || s2 == "1";

            return false;
        }
    }
}
