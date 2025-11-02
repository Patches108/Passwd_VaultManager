using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Services;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {

        private bool _changesMadeSettingsForm = false;
        
        private AppSettings _settings;

        public SettingsWindow() {

            InitializeComponent();

            _settings = SettingsService.Load();
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            SettingsService.Save(_settings);

            // Reload Settings
            string settingsPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PasswordVaultManager",
                "settings.ini"
            );

            SettingsService.Load();

            new ToastNotification("Settings updated.", true).Show();

            SetDialogResultSafe(true);
            Close();
            
        }

        private void SetDialogResultSafe(bool? result) {
            if (this.IsLoaded && this.Owner != null)
                this.DialogResult = result;
        }

        private void CheckUnsavedChanges() {
            if (_changesMadeSettingsForm) {
                var confirm = new YesNoWindow("You have unsaved changes. Do you want to save?");
                bool confirmed = confirm.ShowDialog() == true && confirm.YesNoWin_Result;

                if (confirmed) {
                    Save_Click(this, null);
                }

            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;

            CheckUnsavedChanges();

            Close();
        }

        private void Backup_Click(object sender, RoutedEventArgs e) {
            try {
                AppPaths.EnsureAppDataFolder();

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string destFile = System.IO.Path.Combine(AppPaths.BackupFolder, $"Dreams_Backup_{timestamp}.db");

                using (var source = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={AppPaths.DatabaseFile}"))
                using (var destination = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={destFile}")) {
                    source.Open();
                    destination.Open();
                    source.BackupDatabase(destination);
                }

                new YesNoWindow("Backup completed successfully.").ShowDialog();
                LoadStats();
                Process.Start("explorer.exe", AppPaths.BackupFolder);
            } catch (Exception ex) {
                new YesNoWindow("Backup failed: " + ex.Message).ShowDialog();
            }
        }


        private void Restore_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog {
                Filter = "SQLite Database (*.db)|*.db",
                InitialDirectory = AppPaths.BackupFolder
            };

            //if (ofd.ShowDialog() == true) {
            //    try {
            //        // Close any open DB connections first
            //        GC.Collect();
            //        GC.WaitForPendingFinalizers();

            //        // Overwrite DB file
            //        //File.Copy(ofd.FileName, AppPaths.DatabaseFile, overwrite: true);

            //        new YesNoWindow("Database restored successfully.").ShowDialog();

            //        // Reload context and UI
            //        LoadStats();
            //    } catch (Exception ex) {
            //        new YesNoWindow("Restore failed: " + ex.Message).ShowDialog();
            //    }
            //}
        }


        private void DeleteBackups_Click(object sender, RoutedEventArgs e) {
            if (!Directory.Exists(AppPaths.BackupFolder)) return;

            var confirm = new YesNoWindow("Are you sure you want to delete all backup files?");
            bool confirmed = confirm.ShowDialog() == true && confirm.YesNoWin_Result;

            if (confirmed) {
                try {
                    foreach (var file in Directory.GetFiles(AppPaths.BackupFolder))
                        File.Delete(file);
                    new YesNoWindow("All backups deleted.").ShowDialog();
                    LoadStats();
                } catch (Exception ex) {
                    new YesNoWindow("Failed to delete backups: " + ex.Message).ShowDialog();
                }
            }
        }

        private void OpenBackupsFolder_Click(object sender, RoutedEventArgs e) {
            if (Directory.Exists(AppPaths.BackupFolder)) {
                Process.Start("explorer.exe", AppPaths.BackupFolder);
            } else {
                new YesNoWindow("No backups folder exists.").ShowDialog();
            }
        }

        private void WipeDatabase_Click(object sender, RoutedEventArgs e) {
            var confirmDialog = new YesNoWindow("⚠ This will permanently delete all dream records. Are you absolutely sure?");
            bool confirmed = confirmDialog.ShowDialog() == true && confirmDialog.YesNoWin_Result;

            if (!confirmed) return;

            var inputDialog = new InputDialog();
            bool inputConfirmed = inputDialog.ShowDialog() == true && inputDialog.UserInput == "DELETE";

            if (!inputConfirmed) {
                new YesNoWindow("Wipe cancelled. You must type DELETE exactly.").ShowDialog();
                return;
            }

            try {
                //DatabaseCleaner.ReleaseAllContexts(_navVM);

                //// Ensure all file handles are closed
                //GC.Collect();
                //GC.WaitForPendingFinalizers();

                //if (File.Exists(AppPaths.DatabaseFile)) {
                //    try {
                //        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                //        File.Delete(AppPaths.DatabaseFile);
                //    } catch (Exception ex) {
                //        new YesNoWindow("WOOPS: " + ex.Message).ShowDialog();
                //    }

                //    DatabaseInitializer.CreateDatabase();

                //    new YesNoWindow("Database wiped and recreated.").ShowDialog();
                //    LoadStats();
                //} else {
                //    new YesNoWindow("Database file not found.").ShowDialog();
                //}
            } catch (Exception ex) {
                new YesNoWindow("Failed to wipe database: " + ex.Message).ShowDialog();
            }
        }


        private void OpenBackupFolder_Click(object sender, RoutedEventArgs e) {
            try {
                if (!Directory.Exists(AppPaths.BackupFolder))
                    Directory.CreateDirectory(AppPaths.BackupFolder);

                Process.Start("explorer.exe", AppPaths.BackupFolder);
            } catch (Exception ex) {
                new YesNoWindow($"Failed to open folder:\n{ex.Message}").ShowDialog();
            }
        }

        private void LoadStats() {
            // Count number of records in database
            if (File.Exists(AppPaths.DatabaseFile)) {
                //try {
                //    using var context = new DreamContext();
                //    int recordCount = context.Dreams.Count();
                //    lblRecordCount.Text = $"Number of Records: {recordCount}";
                //} catch {
                //    lblRecordCount.Text = "Number of Records: (error)";
                //}

                //lblDbSize.Text = $"Database Size: {FormatSize(new FileInfo(AppPaths.DatabaseFile).Length)}";
            } else {
                lblRecordCount.Text = "Number of Records: (missing)";
                lblDbSize.Text = "Database Size: (missing)";
            }

            // Backup folder stats
            if (Directory.Exists(AppPaths.BackupFolder)) {
                var files = Directory.GetFiles(AppPaths.BackupFolder);
                long totalSize = files.Sum(f => new FileInfo(f).Length);
                lblBackupSize.Text = $"Backup Folder Size: {FormatSize(totalSize)}";
                NumberOfBackups.Text = $"Total Backups: {files.Length}";
            } else {
                lblBackupSize.Text = "Backup Folder Size: (missing)";
                NumberOfBackups.Text = "Total Backups: 0";
            }
        }

        private string FormatSize(long bytes) {
            if (bytes > 1_000_000)
                return $"{bytes / 1_000_000.0:F2} MB";
            if (bytes > 1_000)
                return $"{bytes / 1_000.0:F2} KB";
            return $"{bytes} B";
        }

        private void InputDialog_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                DialogResult = false;
                CheckUnsavedChanges();
                Close();
            }
        }

        private void frmSettingsLoaded(object sender, RoutedEventArgs e) {
            chkIsSoundEnabled.IsChecked = _settings.SoundEnabled;
            _changesMadeSettingsForm = false;

            tglEnablePin.IsChecked = PinStorage.HasPin();  // reflect current state
        }

        private void chkIsSoundEnabled_Checked(object sender, RoutedEventArgs e) {
            _changesMadeSettingsForm = true;
        }

        private void chkIsSoundEnabled_Unchecked(object sender, RoutedEventArgs e) {
            _changesMadeSettingsForm = true;
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            _changesMadeSettingsForm = true;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                this.DragMove();
            }
        }

        private void RestoreDefaults_Click(object sender, RoutedEventArgs e) {
            comFontList.SelectedItem = "Segoe UI";
            comFontSizeList.SelectedItem = "16.5";

            if(File.Exists(AppPaths.SettingsFile)) {
                File.Delete(AppPaths.SettingsFile);
                RestSettingsHelperFunc();
            } else {
                RestSettingsHelperFunc();
            }            
        }

        private void RestSettingsHelperFunc() {
            string settingsPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PasswordVaultManager",
                    "settings.txt"
                );

            SettingsService.Load();

            new ToastNotification("Settings Restored.", true).Show();
        }

        private void EnablePin_Click(object sender, RoutedEventArgs e) {
            var tgl = (ToggleButton)sender;

            // Enable or change PIN
            if (tgl.IsChecked == true) {
                var reg = new PinRegisterWindow();
                if (reg.ShowDialog() == true) {
                    // PIN saved inside PinStorage.SetPin()
                    new ToastNotification("Pin Created Successfully.", true).Show();
                } else {
                    // user canceled → don't leave toggle on
                    tgl.IsChecked = false;
                }
            }
            // Disable PIN
            else {
                if (PinStorage.HasPin()) {
                    var confirm = new YesNoWindow("Disable PIN protection?");
                    bool confirmed = confirm.ShowDialog() == true && confirm.YesNoWin_Result;

                    if (confirmed) {
                        PinStorage.RemovePin();
                    } else {
                        tgl.IsChecked = true; // keep enabled
                    }
                }
            }
        }
    }
}
