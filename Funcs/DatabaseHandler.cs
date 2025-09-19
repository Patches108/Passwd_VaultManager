using Passwd_VaultManager.Models;
using System.Data.SQLite;
using System.IO;
using System.Windows;

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
    }
}
