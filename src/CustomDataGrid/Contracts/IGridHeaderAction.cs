using System.Windows.Input;
using System.Windows.Media;

namespace CustomDataGrid.Contracts
{
    /// <summary>
    /// A single button in the grid's header toolbar (rendered above the grid by
    /// <c>GridHeaderBar</c>). Used for grid-wide operations such as adding a group
    /// or deleting the current selection.
    /// </summary>
    /// <remarks>
    /// This is intentionally a different contract from <see cref="IGridRowAction"/>.
    /// Header actions operate on the grid as a whole and expose an explicit
    /// <see cref="IsEnabled"/> flag, whereas row actions operate on a specific row
    /// and derive their enabled state from
    /// <see cref="System.Windows.Input.ICommand.CanExecute(object)"/>.
    /// </remarks>
    public interface IGridHeaderAction
    {
        /// <summary>
        /// Gets the text shown on the toolbar button.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Gets the optional icon shown on the button. May be <c>null</c> for a
        /// text-only button.
        /// </summary>
        ImageSource Icon { get; }

        /// <summary>
        /// Gets the command invoked when the button is clicked.
        /// </summary>
        ICommand Command { get; }

        /// <summary>
        /// Gets whether the button is enabled.
        /// </summary>
        bool IsEnabled { get; }
    }
}
