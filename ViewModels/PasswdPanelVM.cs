using Passwd_VaultManager.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Passwd_VaultManager.ViewModels
{
    public class PasswdPanelVM : ViewModelBase
    {
        public static ObservableCollection<AppVault> AppVaults { get; } = new();

        private Brush _borderBrush = Brushes.Transparent;
        private Thickness _borderThickness = new Thickness(0);
        
        private Guid _tempGuid;

        // ICommands
        public ICommand EditSelectedCommand { get; }
        public ICommand DeleteSelectedCommand { get; }
        public ICommand SelectCommand {  get; }

        public PasswdPanelVM() {
            EditSelectedCommand = new RelayCommand(_ => EditSelected(), _ => SelectedAppVault != null);
            DeleteSelectedCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedAppVault != null);
            SelectCommand = new RelayCommand(param => OnSelect(param as AppVault), param => param is AppVault);
        }

        public Brush PanelBorderBrush {
            get => _borderBrush;
            set { _borderBrush = value; OnPropertyChanged(); }
        }

        public Thickness PanelBorderThickness {
            get => _borderThickness;
            set { _borderThickness = value; OnPropertyChanged(); }
        }

        private void OnSelect(AppVault appVault) {
            Guid id = appVault.getAppVaultInstanceGuid;
            SelectedAppVault = appVault;

            // Highlight control
            HightLightPane = true;
        }

        public bool HightLightPane { get; set; }

        private void EditSelected() {
            // open an editor dialog, or toggle an edit mode, etc.
        }

        private void DeleteSelected() {
            if (SelectedAppVault is null) {
                // APPROPIATE ERROR MESSAGE and ERROR HANDLING.
                return;
            }

            AppVaults.Remove(SelectedAppVault);
            SelectedAppVault = AppVaults.FirstOrDefault();
        }

        private AppVault _selectedAppVault;

        public AppVault SelectedAppVault {
            get => _selectedAppVault;
            set {
                if(_selectedAppVault != value) {
                    _selectedAppVault = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AppVault));
                }
            }
        }

        public static ObservableCollection<AppVault> GetAppVaults() {
            return AppVaults;
        }

        public void temporarilyStoreGuid(Guid g) {
            _tempGuid = g;
        }

        public string AppName => SelectedAppVault?.AppName ?? string.Empty;
    }
}
