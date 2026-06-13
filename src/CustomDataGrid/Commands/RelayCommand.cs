using System;
using System.Windows.Input;

namespace CustomDataGrid.Commands
{
    /// <summary>
    /// A minimal <see cref="ICommand"/> implementation that delegates
    /// <see cref="Execute"/> and <see cref="CanExecute"/> to supplied delegates.
    /// Used to provide the default behavior of <see cref="Controls.GridControl"/>'s
    /// inbound command dependency properties.
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The action invoked by <see cref="Execute"/>. Must not be <c>null</c>.</param>
        /// <param name="canExecute">
        /// The predicate invoked by <see cref="CanExecute"/>, or <c>null</c> to
        /// always allow execution.
        /// </param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <inheritdoc/>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <inheritdoc/>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Raises <see cref="CanExecuteChanged"/> for all commands, prompting WPF
        /// to re-query <see cref="CanExecute"/>.
        /// </summary>
        public static void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
