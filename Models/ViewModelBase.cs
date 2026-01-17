using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Passwd_VaultManager.Models {

    /// <summary>
    /// Base class for ViewModels that provides property change
    /// notification support for data binding.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged {



        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;



        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified
        /// property name.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that changed. This value is supplied
        /// automatically by the compiler when omitted.
        /// </param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
