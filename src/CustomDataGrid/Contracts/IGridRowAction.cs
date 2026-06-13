using System.Windows.Input;
using System.Windows.Media;

namespace CustomDataGrid.Contracts
{
    /// <summary>
    /// A single entry in a row's actions menu (the popup opened from the actions
    /// column). A grid exposes two independent lists of these: one for group rows
    /// and one for item rows.
    /// </summary>
    /// <remarks>
    /// An action's enabled state is not a property here; it is resolved through
    /// <see cref="System.Windows.Input.ICommand.CanExecute(object)"/>, which the
    /// control calls passing the row (an <see cref="IGridRow"/>) as the parameter.
    /// This lets the same action be enabled for some rows and disabled for others.
    /// </remarks>
    public interface IGridRowAction
    {
        /// <summary>
        /// Gets the text shown for this menu entry. Ignored when
        /// <see cref="IsSeparator"/> is <c>true</c>.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Gets the optional icon shown beside the label. May be <c>null</c> for
        /// a text-only entry.
        /// </summary>
        ImageSource Icon { get; }

        /// <summary>
        /// Gets the command invoked when the entry is clicked. The control passes
        /// the owning <see cref="IGridRow"/> as the command parameter and uses
        /// <see cref="System.Windows.Input.ICommand.CanExecute(object)"/> to
        /// determine the entry's enabled state. May be <c>null</c> when
        /// <see cref="IsSeparator"/> is <c>true</c>.
        /// </summary>
        ICommand Command { get; }

        /// <summary>
        /// Gets whether this entry is a visual separator rather than a clickable
        /// action. When <c>true</c>, <see cref="Label"/>, <see cref="Icon"/>, and
        /// <see cref="Command"/> are ignored.
        /// </summary>
        bool IsSeparator { get; }
    }
}
