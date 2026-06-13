using System.Collections.Generic;
using System.Windows;

namespace CustomDataGrid.Contracts.Events
{
    /// <summary>
    /// Routed event data raised by the grid when the multi-selection changes,
    /// reporting which rows were added to and removed from the selection.
    /// </summary>
    public class SelectedRowsChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedRowsChangedEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event being raised.</param>
        /// <param name="addedRows">The rows added to the selection. A <c>null</c> value is treated as empty.</param>
        /// <param name="removedRows">The rows removed from the selection. A <c>null</c> value is treated as empty.</param>
        public SelectedRowsChangedEventArgs(
            RoutedEvent routedEvent,
            IList<IGridRow> addedRows,
            IList<IGridRow> removedRows)
            : base(routedEvent)
        {
            AddedRows = addedRows ?? new List<IGridRow>();
            RemovedRows = removedRows ?? new List<IGridRow>();
        }

        /// <summary>
        /// Gets the rows that were added to the selection. Never <c>null</c>.
        /// </summary>
        public IList<IGridRow> AddedRows { get; }

        /// <summary>
        /// Gets the rows that were removed from the selection. Never <c>null</c>.
        /// </summary>
        public IList<IGridRow> RemovedRows { get; }
    }
}
