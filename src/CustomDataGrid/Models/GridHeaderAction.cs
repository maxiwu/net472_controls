using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using CustomDataGrid.Contracts;

namespace CustomDataGrid.Models
{
    /// <summary>
    /// The default implementation of <see cref="IGridHeaderAction"/>. Used to
    /// populate the buttons in the grid's header toolbar.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <see cref="IsEnabled"/> is never assigned, it is derived from
    /// <see cref="ICommand.CanExecute(object)"/> on the bound command (passing
    /// <c>null</c> as the parameter, since header actions are not
    /// row-scoped). The action subscribes to <see cref="ICommand.CanExecuteChanged"/>
    /// and raises <see cref="PropertyChanged"/> for <see cref="IsEnabled"/> so
    /// data-bound buttons update automatically.
    /// </para>
    /// <para>
    /// Once <see cref="IsEnabled"/> has been assigned explicitly, that value
    /// takes precedence and the command's <c>CanExecute</c> is no longer
    /// consulted.
    /// </para>
    /// </remarks>
    public class GridHeaderAction : IGridHeaderAction, INotifyPropertyChanged
    {
        private bool? _explicitIsEnabled;

        /// <summary>
        /// Initializes a new header action.
        /// </summary>
        /// <param name="label">The text shown on the toolbar button.</param>
        /// <param name="command">The command invoked when the button is clicked.</param>
        /// <param name="icon">The optional icon shown on the button. May be <c>null</c>.</param>
        public GridHeaderAction(string label, ICommand command, ImageSource icon = null)
        {
            Label = label;
            Command = command;
            Icon = icon;

            if (command != null)
            {
                command.CanExecuteChanged += OnCommandCanExecuteChanged;
            }
        }

        /// <inheritdoc/>
        public string Label { get; }

        /// <inheritdoc/>
        public ImageSource Icon { get; }

        /// <inheritdoc/>
        public ICommand Command { get; }

        /// <summary>
        /// Gets or sets whether the button is enabled. When this property has
        /// not been assigned, the value is derived from
        /// <see cref="ICommand.CanExecute(object)"/> on <see cref="Command"/>.
        /// Assigning the property fixes the value and disables the
        /// <c>CanExecute</c> fallback.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                if (_explicitIsEnabled.HasValue) return _explicitIsEnabled.Value;
                return Command != null && Command.CanExecute(null);
            }
            set
            {
                if (_explicitIsEnabled.HasValue && _explicitIsEnabled.Value == value) return;
                _explicitIsEnabled = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            if (_explicitIsEnabled.HasValue) return;
            OnPropertyChanged(nameof(IsEnabled));
        }

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the given property name.
        /// </summary>
        /// <param name="propertyName">The property name; supplied automatically
        /// by the compiler when called from a property setter.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
