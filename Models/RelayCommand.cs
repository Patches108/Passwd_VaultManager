using System.Windows.Input;

namespace Passwd_VaultManager.Models {
    /// <summary>
    /// A reusable implementation of <see cref="ICommand"/> that delegates
    /// execution and availability logic to provided callbacks.
    /// 
    /// Commonly used in MVVM to bind UI actions to ViewModel methods
    /// without requiring code-behind.
    /// </summary>
    public sealed class RelayCommand : ICommand {

        private readonly Predicate<object?>? _canExecute;
        private readonly Action<object?> _execute;



        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">
        /// The action to execute when the command is invoked.
        /// </param>
        /// <param name="canExecute">
        /// Optional predicate that determines whether the command can execute.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="execute"/> is <c>null</c>.
        /// </exception>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }



        /// <summary>
        /// Forces WPF to re-evaluate whether the command can execute.
        /// </summary>
        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();


        /// <summary>
        /// Determines whether the command can execute with the given parameter.
        /// </summary>
        /// <param name="parameter">Optional command parameter.</param>
        /// <returns>
        /// <c>true</c> if the command can execute; otherwise <c>false</c>.
        /// </returns>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;



        /// <summary>
        /// Executes the command logic with the given parameter.
        /// </summary>
        /// <param name="parameter">Optional command parameter.</param>
        public void Execute(object? parameter) => _execute(parameter);



        /// <summary>
        /// Occurs when changes affect whether the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
