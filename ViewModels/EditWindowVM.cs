using Passwd_VaultManager.Models;
using Passwd_VaultManager.Views;
using System.Windows.Input;

namespace Passwd_VaultManager.ViewModels
{
    internal sealed class EditWindowVM : ViewModelBase {

        private string _appName = String.Empty;
        private string _userName = String.Empty;
        private string _appPasswd = String.Empty;

        private bool _isUserNameSet = false;
        private bool _isPasswdSet = false;

        private bool _editWin_ChangesMade = false;

        public EditWindowVM() {
            
        }

        public EditWindowVM(AppVault appVault) {
            _appName = appVault?.AppName ?? String.Empty;
            _userName = appVault?.UserName ?? String.Empty;
            _appPasswd = appVault?.Password ?? String.Empty;
        }

        // GETTERS AND SETTERS
        public string AppName { 
            get => _appName; 
            set { 
                if (_appName == value) return; 
                _appName = value; 
                OnPropertyChanged(); 
            } 
        }        

        public string Username {
            get => _userName; 
            set {
                if (_userName == value) return;
                _userName = value; 
                OnPropertyChanged();
            }
        }

        public string Password {
            get => _appPasswd; 
            set {
                if (_appPasswd == value) return;
                _appPasswd = value;
                OnPropertyChanged();
            }
        }

        public bool UserNameSet {
            get => _isUserNameSet; 
            set {
                if (_isUserNameSet == value) return;
                _isUserNameSet = value;
                OnPropertyChanged();
            }
        }

        public bool PasswdSet {
            get => _isPasswdSet; 
            set { 
                if(_isPasswdSet == value) return;
                _isPasswdSet = value;
                OnPropertyChanged();
            }
        }

        public bool ChangesMade {
            get => _editWin_ChangesMade;
            set {
                if (_editWin_ChangesMade == value) return;
                _editWin_ChangesMade = value;
                OnPropertyChanged();
            }
        }

    }
}
