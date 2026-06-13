using System.Windows;

namespace CustomDataGrid.Contracts.Events
{
    /// <summary>
    /// Routed event data raised by the grid when a cell edit is cancelled
    /// (for example, the user presses Escape).
    /// </summary>
    public class CellEditCancelledEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CellEditCancelledEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event being raised.</param>
        /// <param name="row">The row whose cell edit was cancelled.</param>
        /// <param name="columnName">The name of the column whose edit was cancelled.</param>
        public CellEditCancelledEventArgs(RoutedEvent routedEvent, IGridRow row, string columnName)
            : base(routedEvent)
        {
            Row = row;
            ColumnName = columnName;
        }

        /// <summary>
        /// Gets the row whose cell edit was cancelled.
        /// </summary>
        public IGridRow Row { get; }

        /// <summary>
        /// Gets the name of the column whose edit was cancelled.
        /// </summary>
        public string ColumnName { get; }
    }
}
