using System.Windows.Input;
using System.Windows.Media;
using CustomDataGrid.Contracts;

namespace CustomDataGrid.Models
{
    /// <summary>
    /// The default implementation of <see cref="IGridRowAction"/>. Used to
    /// populate the actions menu attached to group rows and item rows.
    /// </summary>
    /// <remarks>
    /// To add a visual divider between menu entries, use the
    /// <see cref="Separator"/> factory rather than constructing a
    /// <see cref="GridRowAction"/> directly with no label or command.
    /// </remarks>
    public class GridRowAction : IGridRowAction
    {
        /// <summary>
        /// Initializes a new clickable menu entry.
        /// </summary>
        /// <param name="label">The text shown on the entry.</param>
        /// <param name="command">The command invoked when the entry is clicked.
        /// The control passes the owning <see cref="IGridRow"/> as the parameter.</param>
        /// <param name="icon">The optional icon shown beside the label. May be <c>null</c>.</param>
        public GridRowAction(string label, ICommand command, ImageSource icon = null)
        {
            Label = label;
            Command = command;
            Icon = icon;
            IsSeparator = false;
        }

        private GridRowAction()
        {
            IsSeparator = true;
        }

        /// <summary>
        /// Gets a new separator entry. A separator has no label, icon, or
        /// command and is rendered as a visual divider in the menu.
        /// </summary>
        public static GridRowAction Separator
        {
            get { return new GridRowAction(); }
        }

        /// <inheritdoc/>
        public string Label { get; }

        /// <inheritdoc/>
        public ImageSource Icon { get; }

        /// <inheritdoc/>
        public ICommand Command { get; }

        /// <inheritdoc/>
        public bool IsSeparator { get; }
    }
}
