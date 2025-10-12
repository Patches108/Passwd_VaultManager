using Passwd_VaultManager.Funcs;

namespace Passwd_VaultManager {
    public static class Globals {
        
        // Global constants / read-only values
        //public static readonly int PASSWORD_SYMBOL_COUNT;

        public static string AppName => "Password Vault Manager";
        public static string Version => "1.0.0";

        // Static constructor (runs once at app startup)
        static Globals() {
            //PASSWORD_SYMBOL_COUNT = PasswdGen.TotalNumSymbols;
        }
    }
}
