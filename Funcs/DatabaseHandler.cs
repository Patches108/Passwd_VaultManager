using Passwd_VaultManager.Models;
using System.Data.SQLite;
using System.IO;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Data.Common;

namespace Passwd_VaultManager.Funcs {
    public static class DatabaseHandler {

        private static readonly string _connectionString =
            $"Data Source={System.IO.Path.Combine(AppPaths.AppDataFolder, "Vault.db")};Version=3";

        private static readonly string _dbLocation = AppPaths.AppDataFolder + "\\Vault.db";

        public static void initDatabase() {
            if(!File.Exists(_dbLocation)) {
                SQLiteConnection.CreateFile(@""+ _dbLocation);

                using (var conn = new SQLiteConnection(_connectionString)) { 
                    conn.Open();

                    string CreateStandardDatabaseQuery = @"
                        CREATE TABLE IF NOT EXISTS Vault (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            DateCreated TEXT NOT NULL,
                            AppName TEXT NOT NULL,
                            UserName TEXT NOT NULL,
                            Passwd TEXT NOT NULL,
                            IsUserNameSet TEXT NOT NULL,
                            IsPasswdSet TEXT NOT NULL
                        );";

                    using (var comm = new SQLiteCommand(conn)) { 
                        comm.CommandText = CreateStandardDatabaseQuery;
                        comm.ExecuteNonQuery();
                    }
                }

                //MessageBox.Show("DB created!");
            }
        }

        public static void WriteRecordToDatabase() {

        }

        public static void ReadRecordFromDatabase() {

        }

        public static void SearchDBByID(int id) {

        }

        public static async Task<ObservableCollection<AppVault>> GetVaults() {
            ObservableCollection<AppVault> vaults = new ObservableCollection<AppVault>();   // List of vault to return

            await using SQLiteConnection conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            using var comm = conn.CreateCommand();
            comm.CommandText = "SELECT DateCreated, AppName, UserName, Passwd, IsUserNameSet, IsPasswdSet FROM Vault";

            await using DbDataReader reader = await comm.ExecuteReaderAsync();

            while (await reader.ReadAsync()) {
                vaults.Add(new AppVault() {
                    AppName = reader.GetString(0),
                    UserName = reader.GetString(1),
                    Password = reader.GetString(2),
                    IsUserNameSet = reader.GetBoolean(3),       // Will it convert text to bool?
                    IsPasswdSet = reader.GetBoolean(4)          // Will it convert text to bool?
                });
            }

            return vaults;
        }
    }
}
