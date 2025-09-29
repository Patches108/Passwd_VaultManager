namespace Passwd_VaultManager.Utils {
    public interface IMessageService {
        void Info(string message, string title = "Info");
        void Error(string message, string title = "Error");
    }
}
