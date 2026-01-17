using Microsoft.Win32;
using Passwd_VaultManager.Funcs;
using Passwd_VaultManager.Models;
using Passwd_VaultManager.Services;
using Passwd_VaultManager.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Passwd_VaultManager.Views {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {

        private bool _changesMadeSettingsForm = false;
        
        private readonly SettingsWindowVM _vm = new();

        //private AppSettings _settings;

        private Func<Task> _refreshAction;

        public SettingsWindow(Func<Task> _refreshAction) {

            InitializeComponent();

            DataContext = _vm;

            //_settings = SettingsService.Load();

            this._refreshAction = _refreshAction;
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            SettingsService.Save(App.Settings);

            // Reload Settings
            string settingsPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PasswordVaultManager",
                "settings.ini"
            );

            SettingsService.Load();

            new ToastNotification("Settings updated.", true, SoundController.SuccessSound).Show();

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

                // Ensure all file handles are closed
                GC.Collect();
                GC.WaitForPendingFinalizers();

                LoadStats();
                Process.Start("explorer.exe", AppPaths.BackupFolder);
                new ToastNotification("Backup completed successfully.", true, SoundController.SuccessSound).Show(); // Place at end (hogs UI)
            } catch (Exception ex) {
                new MessageWindow("Backup failed: " + ex.Message, SoundController.ErrorSound).ShowDialog();
            }
        }


        private void Restore_Click(object sender, RoutedEventArgs e) {

            OpenFileDialog ofd = new OpenFileDialog {
                Filter = "SQLite Database (*.db)|*.db",
                InitialDirectory = AppPaths.BackupFolder
            };

            if (ofd.ShowDialog() == true) {
                try {
                    // Close any open DB connections first
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Overwrite DB file
                    File.Copy(ofd.FileName, AppPaths.DatabaseFile, overwrite: true);

                    // Reload context and UI
                    LoadStats();

                    new ToastNotification("Backup completed successfully.", true, SoundController.SuccessSound).Show(); // Place at end (hogs UI)

                } catch (Exception ex) {
                    new MessageWindow("Restore failed: " + ex.Message, SoundController.ErrorSound).ShowDialog();
                }
            }
        }


        private void DeleteBackups_Click(object sender, RoutedEventArgs e) {
            if (!Directory.Exists(AppPaths.BackupFolder)) return;

            var confirm = new YesNoWindow("Are you sure you want to delete all backup files?");
            bool confirmed = confirm.ShowDialog() == true && confirm.YesNoWin_Result;

            if (!confirmed) return;

            try {
                // Forcefully release potential SQLite handles
                GC.Collect();
                GC.WaitForPendingFinalizers();

                foreach (var file in Directory.GetFiles(AppPaths.BackupFolder)) {
                    bool deleted = false;
                    int attempts = 0;

                    while (!deleted && attempts < 2) {
                        try {
                            File.Delete(file);
                            deleted = true;
                        } catch (IOException) {
                            attempts++;
                            Thread.Sleep(200);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        } catch (UnauthorizedAccessException) {
                            attempts++;
                            Thread.Sleep(200);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                    }

                    if (!deleted) {
                        new ToastNotification($"Failed to delete backup: {file}", false, SoundController.ErrorSound).Show();
                        new MessageWindow($"Failed to delete backup: {file}\n\nRestart the app to fully break all file handles and try again.", SoundController.ErrorSound).ShowDialog();
                    }
                }

                new ToastNotification("All backups deleted.", true, SoundController.SuccessSound).Show();
                LoadStats();
            } catch (Exception ex) {
                new MessageWindow("Failed to delete backups: " + ex.Message, SoundController.ErrorSound).ShowDialog();
            }
        }


        private void OpenBackupsFolder_Click(object sender, RoutedEventArgs e) {
            if (Directory.Exists(AppPaths.BackupFolder)) {
                Process.Start("explorer.exe", AppPaths.BackupFolder);
            } else {
                // try creaste folder and do it again.
                try {
                    AppPaths.EnsureAppDataFolder();
                    Process.Start("explorer.exe", AppPaths.BackupFolder);
                } catch (Exception ex) {
                    new MessageWindow("Failed to create and open Backups folder.", SoundController.ErrorSound).ShowDialog();
                }
            }
        }

        private void WipeDatabase_Click(object sender, RoutedEventArgs e) {
            var confirmDialog = new YesNoWindow("⚠ This will permanently delete all dream records. Are you absolutely sure?");
            bool confirmed = confirmDialog.ShowDialog() == true && confirmDialog.YesNoWin_Result;

            if (!confirmed) return;

            var inputDialog = new InputDialog();
            bool inputConfirmed = inputDialog.ShowDialog() == true && inputDialog.UserInput == "DELETE";

            if (!inputConfirmed) {
                new MessageWindow("Wipe cancelled. You must type DELETE exactly.", SoundController.ErrorSound).ShowDialog();
                return;
            }

            try {

                // Ensure all file handles are closed
                GC.Collect();
                GC.WaitForPendingFinalizers();

                if (File.Exists(AppPaths.DatabaseFile)) {
                    try {
                        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                        File.Delete(AppPaths.DatabaseFile);
                    } catch (Exception ex) {
                        new MessageWindow("Could not delete Database:\n\n" + ex.Message, SoundController.ErrorSound).ShowDialog();
                    }

                    File.Delete(AppPaths.DatabaseFile);     // Delete DB file
                    DatabaseHandler.initDatabase();         // remake the DB file anew.

                    new ToastNotification("Database wiped and recreated.", true, SoundController.SuccessSound).Show();
                    LoadStats();
                } else {
                    new MessageWindow("Database file not found.", SoundController.ErrorSound).ShowDialog();
                }
            } catch (Exception ex) {
                new MessageWindow("Failed to wipe database:\n\n" + ex.Message, SoundController.ErrorSound).ShowDialog();
            }
        }


        private void OpenBackupFolder_Click(object sender, RoutedEventArgs e) {
            try {
                if (!Directory.Exists(AppPaths.BackupFolder))
                    Directory.CreateDirectory(AppPaths.BackupFolder);

                Process.Start("explorer.exe", AppPaths.BackupFolder);
            } catch (Exception ex) {
                new MessageWindow($"Failed to open folder:\n\n{ex.Message}", SoundController.ErrorSound).ShowDialog();
            }
        }

        private async void LoadStats() {
            // Count number of records in database
            if (File.Exists(AppPaths.DatabaseFile)) {
                try {
                    int recordCount = await DatabaseHandler.GetRecordCountAsync();
                    lblRecordCount.Text = $"Number of Records: {recordCount}";
                } catch (Exception ex) {
                    lblRecordCount.Text = "Number of Records: (error)";
                    new ToastNotification($"ERROR: {ex.Message}", false, SoundController.ErrorSound).Show();
                }

                lblDbSize.Text = $"Database Size: {FormatSize(new FileInfo(AppPaths.DatabaseFile).Length)}";

                await _refreshAction();
            } else {
                lblRecordCount.Text = "Number of Records: (DB missing)";
                lblDbSize.Text = "Database Size: (N/A)";
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
                CheckUnsavedChanges();
                Close();
            }
        }

        private void frmSettingsLoaded(object sender, RoutedEventArgs e) {
            chkIsSoundEnabled.IsChecked = App.Settings.SoundEnabled;
            _changesMadeSettingsForm = false;

            tglEnablePin.IsChecked = PinStorage.HasPin();  // reflect current state

            // set font family and size to whats in settings.
            //comFontList.SelectedItem = App.Settings.FontFamily; // WHATS IN SETTINGS.
            //comFontSizeList.SelectedItem = App.Settings.FontSize;// WHATS IN SETTINGS.

            _vm.SelectedFont = App.Settings.FontFamily;
            _vm.SelectedFontSize = App.Settings.FontSize;
            _vm.SoundEnabled = App.Settings.SoundEnabled;

            SharedFuncs.Apply(this, App.Settings);

            LoadStats();

            _changesMadeSettingsForm = false;
        }

        private void chkIsSoundEnabled_Checked(object sender, RoutedEventArgs e) {
            _changesMadeSettingsForm = true;
            App.Settings.SoundEnabled = true;
        }

        private void chkIsSoundEnabled_Unchecked(object sender, RoutedEventArgs e) {
            _changesMadeSettingsForm = true;
            App.Settings.SoundEnabled = false;
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            _changesMadeSettingsForm = true;
            App.Settings.FontFamily = comFontList.SelectedItem.ToString();
            SharedFuncs.Apply(this, App.Settings);
        }

        private void DragWindow(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                this.DragMove();
            }
        }

        private void RestoreDefaults_Click(object sender, RoutedEventArgs e) {
            comFontList.SelectedItem = "Segoe UI";
            comFontSizeList.SelectedItem = "15";

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
                    "settings.ini"
                );

            SettingsService.Load();

            new ToastNotification("Settings Restored.", true, SoundController.SuccessSound).Show();
        }

        private void EnablePin_Click(object sender, RoutedEventArgs e) {
            var tgl = (ToggleButton)sender;

            // Enable or change PIN
            if (tgl.IsChecked == true) {
                var reg = new PinRegisterWindow();
                if (reg.ShowDialog() == true) {
                    // PIN saved inside PinStorage.SetPin()
                    new ToastNotification("Pin Created Successfully.", true, SoundController.SuccessSound).Show();
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
                        new ToastNotification("Pin Removed Successfully.", true, SoundController.SuccessSound).Show();
                    } else {
                        tgl.IsChecked = true; // keep enabled
                    }
                }
            }
        }

        private void cmdResetHelperBot_Click(object sender, RoutedEventArgs e) {
            string res = SettingsService.ResetHelperBot(App.Settings).Trim();

            if (res == String.Empty) {
                //new ToastNotification("Thanks for resetting me!", true).Show();
                new Helper("Thanks for resetting me!", SoundController.SuccessSound).Show();
            } else {
                new MessageWindow($"ERROR: Opps! There was an error updating the helper system:\n\n{res}", SoundController.ErrorSound).Show();
            }
        }

        private void Settings_Closing(object sender, CancelEventArgs e) {
            if (Application.Current.MainWindow is MainWindow main) {
                main.ApplyFontsFromSettingsWin();
            } else {
                new MessageWindow("Could not set fonts to mainwindow", SoundController.ErrorSound);
            }
        }

        private void com_FontSizeChanged(object sender, SelectionChangedEventArgs e) {
            if (!IsLoaded) return;

            if (comFontSizeList.SelectedItem is not double size) return;

            App.Settings.FontSize = size;
            SharedFuncs.Apply(this, App.Settings);
        }
    }
}
