using Microsoft.Data.Sqlite;
using Passwd_VaultManager.Models;
using System.Collections.ObjectModel;
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
                            UserName       BLOB    NULL,   
                            Passwd         BLOB    NULL,   
                            IsUserNameSet  INTEGER NOT NULL,   
                            IsPasswdSet    INTEGER NOT NULL,
                            ExcludedChars  BLOB    NULL,
                            BitRate        INTEGER NOT NULL    
                        );
                        ";

                    using (var comm = new SQLiteCommand(conn)) { 
                        comm.CommandText = CreateStandardDatabaseQuery;
                        comm.ExecuteNonQuery();
                    }
                }
            }
        }

        public static async Task<long> WriteRecordToDatabaseAsync(AppVault v, CancellationToken ct = default) {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(ct);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Vault
                    (DateCreated, AppName, UserName, Passwd, IsUserNameSet, IsPasswdSet, ExcludedChars, BitRate)
                VALUES
                    ($date, $app, $user, $pwd, $userSet, $pwdSet, $excluded, $bitR);
                SELECT last_insert_rowid();";

            cmd.Parameters.AddWithValue("$date", DateTime.UtcNow.ToString("o")); // ISO 8601 UTC

            cmd.Parameters.AddWithValue("$app", EncryptionService.EncryptToBlob(v.AppName));
            cmd.Parameters.AddWithValue("$user", EncryptionService.EncryptToBlob(v.UserName ?? ""));
            cmd.Parameters.AddWithValue("$pwd", EncryptionService.EncryptToBlob(v.Password ?? ""));
            cmd.Parameters.AddWithValue("$userSet", v.IsUserNameSet ? 1 : 0);
            cmd.Parameters.AddWithValue("$pwdSet", v.IsPasswdSet ? 1 : 0);
            cmd.Parameters.AddWithValue("$excluded", EncryptionService.EncryptToBlob(v.ExcludedChars ?? ""));
            cmd.Parameters.AddWithValue("$bitR", v.BitRate);

            var scalar = await cmd.ExecuteScalarAsync(ct);
            return scalar switch {
                long id => id,
                _ => Convert.ToInt64(scalar)
            };
        }

        // extra salting.
        //private static byte[] GetAppEntropy() =>
        //    Encoding.UTF8.GetBytes("PasswdVaultManager-Entropy-Q9x2Tb7Lm4RjK8Vp-v1");

        public static async Task<ObservableCollection<AppVault>> GetVaults(CancellationToken ct = default) {
            var vaults = new ObservableCollection<AppVault>();

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(ct);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, DateCreated, AppName, UserName, Passwd,
                       IsUserNameSet, IsPasswdSet, ExcludedChars, BitRate
                FROM Vault;";

            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct)) {
                var app = new AppVault {
                    Id = reader.GetInt64(reader.GetOrdinal("id")),
                    AppName = DecryptBlobSafe(reader, reader.GetOrdinal("AppName")),
                    UserName = DecryptBlobSafe(reader, reader.GetOrdinal("UserName")),
                    Password = DecryptBlobSafe(reader, reader.GetOrdinal("Passwd")),
                    ExcludedChars = DecryptBlobSafe(reader, reader.GetOrdinal("ExcludedChars")),
                    IsUserNameSet = ReadBoolFlexible(reader, reader.GetOrdinal("IsUserNameSet")),
                    IsPasswdSet = ReadBoolFlexible(reader, reader.GetOrdinal("IsPasswdSet")),
                    BitRate = reader.GetInt32(reader.GetOrdinal("BitRate")),
                    DateCreated = DateTime.TryParse(reader.GetString(reader.GetOrdinal("DateCreated")), out var dc) ? dc : null
                };

                app.IsStatusGood = app.IsUserNameSet && app.IsPasswdSet;
                vaults.Add(app);
            }

            return vaults;
        }

        public static async Task<int> UpdateVaultByIdAsync(
    long id,
    string appName,
    string? userName,
    string? password,
    bool isUserNameSet,
    bool isPasswdSet,
    string excluded,
    int bitRate,
    CancellationToken ct = default) {

            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(ct);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Vault
                SET
                    AppName        = $app,
                    UserName       = $user,
                    Passwd         = $pwd,
                    IsUserNameSet  = $userSet,
                    IsPasswdSet    = $pwdSet,
                    ExcludedChars  = $excluded,
                    BitRate        = $bitR
                WHERE id = $id;";

            // encrypted blobs
            cmd.Parameters.AddWithValue("$app", EncryptionService.EncryptToBlob(appName ?? string.Empty));
            cmd.Parameters.AddWithValue("$user", EncryptionService.EncryptToBlob(userName ?? string.Empty));
            cmd.Parameters.AddWithValue("$pwd", EncryptionService.EncryptToBlob(password ?? string.Empty));
            cmd.Parameters.AddWithValue("$excluded", EncryptionService.EncryptToBlob(excluded ?? string.Empty));

            // plain ints/bools
            cmd.Parameters.AddWithValue("$userSet", isUserNameSet ? 1 : 0);
            cmd.Parameters.AddWithValue("$pwdSet", isPasswdSet ? 1 : 0);

            cmd.Parameters.AddWithValue("$bitR", bitRate);
            cmd.Parameters.AddWithValue("$id", id);

            // returns number of rows affected (0 = not found / nothing updated, 1 = success)
            return await cmd.ExecuteNonQueryAsync(ct);
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

        public static async Task<int> DeleteVaultAsync(
    string appName,
    string userName,
    string password,
    CancellationToken ct = default) 
            {

            // Normalize inputs for exact, case-sensitive match. Adjust if you want OrdinalIgnoreCase.
            appName = appName?.Trim() ?? string.Empty;
            userName = userName?.Trim() ?? string.Empty;
            password = password?.Trim() ?? string.Empty;

            var idsToDelete = new List<long>();

            await using (var conn = new SqliteConnection(_connectionString)) {
                await conn.OpenAsync(ct);

                // 1) Read candidate rows (just what we need)
                using (var select = conn.CreateCommand()) {
                    select.CommandText = @"SELECT id, AppName, UserName, Passwd FROM Vault;";
                    await using var reader = await select.ExecuteReaderAsync(ct);

                    int ordId = reader.GetOrdinal("id");
                    int ordApp = reader.GetOrdinal("AppName");
                    int ordUser = reader.GetOrdinal("UserName");
                    int ordPwd = reader.GetOrdinal("Passwd");

                    while (await reader.ReadAsync(ct)) {
                        // Decrypt each blob and compare to plaintext criteria
                        string dbApp = DecryptBlobSafe(reader, ordApp);
                        string dbUser = DecryptBlobSafe(reader, ordUser);
                        string dbPwd = DecryptBlobSafe(reader, ordPwd);

                        if (string.Equals(dbApp, appName, StringComparison.Ordinal) &&
                            string.Equals(dbUser, userName, StringComparison.Ordinal) &&
                            string.Equals(dbPwd, password, StringComparison.Ordinal)) {
                            // Capture this row's id to delete later
                            long id = reader.GetInt64(ordId);
                            idsToDelete.Add(id);
                        }
                    }
                }

                if (idsToDelete.Count == 0) return 0;

                // 2) Delete matches inside a transaction
                await using var tx = await conn.BeginTransactionAsync(ct);
                int total = 0;

                using (var del = conn.CreateCommand()) {
                    del.CommandText = "DELETE FROM Vault WHERE id = $id;";
                    var p = del.CreateParameter();
                    p.ParameterName = "$id";
                    del.Parameters.Add(p);

                    foreach (long id in idsToDelete) {
                        p.Value = id;
                        total += await del.ExecuteNonQueryAsync(ct);
                    }
                }

                await tx.CommitAsync(ct);
                return total;
            }
        }
        // Convenience update overload
        public static Task<int> UpdateVaultAsync(AppVault v, CancellationToken ct = default) {
            if (v == null)
                throw new ArgumentNullException(nameof(v));

            return UpdateVaultByIdAsync(
                id: v.Id,
                appName: v.AppName,
                userName: v.UserName,
                password: v.Password,
                isUserNameSet: v.IsUserNameSet,
                isPasswdSet: v.IsPasswdSet,
                excluded: v.ExcludedChars,
                bitRate: v.BitRate,
                ct: ct);
        }



        // Convenience overload: delete using an AppVault instance (SelectedAppVault, etc.)
        public static Task<int> DeleteVaultAsync(AppVault v, CancellationToken ct = default)
            => DeleteVaultAsync(v?.AppName ?? "", v?.UserName ?? "", v?.Password ?? "", ct);

        // If have  row id, this is faster:
        public static async Task<int> DeleteVaultByIdAsync(long id, CancellationToken ct = default) {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(ct);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Vault WHERE id = $id;";
            cmd.Parameters.AddWithValue("$id", id);

            return await cmd.ExecuteNonQueryAsync(ct);
        }

        public static async Task<int> GetRecordCountAsync(CancellationToken ct = default) {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(ct);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Vault;";

            var result = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt32(result);
        }
    }
}
