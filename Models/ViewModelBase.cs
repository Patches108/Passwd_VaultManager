using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Passwd_VaultManager.Models {

    public abstract class ViewModelBase : INotifyPropertyChanged {

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event to notify subscribers that a property value has changed.
        /// </summary>
        /// <remarks>This method is typically called by property setters to notify data-binding clients or
        /// other listeners  that a property value has been updated. The <see cref="PropertyChanged"/> event will not be
        /// raised if there are no subscribers.</remarks>
        /// <param name="propertyName">The name of the property that changed. This parameter is optional and defaults to the name of the caller if
        /// not specified.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
