using Passwd_VaultManager.Models;
using Passwd_VaultManager.Views;
using System.Windows.Input;

namespace Passwd_VaultManager.ViewModels
{
    internal class EditWindowVM : ViewModelBase {

        private string _appName = String.Empty;
        private string _userName = String.Empty;
        private string _appPasswd = String.Empty;

        private bool _isUserNameSet = false;
        private bool _isPasswdSet = false;

        private bool _editWin_ChangesMade = false;

        public EditWindowVM() {
            
        }

        // GETTERS AND SETTERS
        public string AppName {
            get { return _appName; }
            set { _appName = value; }
        }

        public string Username {
            get { return _appName; }
            set { _appName = value; }
        }

        public string Password {
            get { return _appName; }
            set { _appName = value; }
        }

        public bool UserNameSet {
            get { return _isUserNameSet; }
            set { _isUserNameSet = value; }
        }

        public bool PasswdSet {
            get { return _isPasswdSet; }
            set { _isPasswdSet = value; }
        }

    }
}
