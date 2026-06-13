using System.Windows;

namespace CustomDataGrid.Contracts.Events
{
    /// <summary>
    /// Routed event data raised by the grid when the single selected row changes.
    /// </summary>
    public class SelectedRowChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedRowChangedEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event being raised.</param>
        /// <param name="oldRow">The previously selected row, or <c>null</c> if none.</param>
        /// <param name="newRow">The newly selected row, or <c>null</c> if the selection was cleared.</param>
        public SelectedRowChangedEventArgs(RoutedEvent routedEvent, IGridRow oldRow, IGridRow newRow)
            : base(routedEvent)
        {
            OldRow = oldRow;
            NewRow = newRow;
        }

        /// <summary>
        /// Gets the previously selected row. May be <c>null</c>.
        /// </summary>
        public IGridRow OldRow { get; }

        /// <summary>
        /// Gets the newly selected row. May be <c>null</c> when the selection was cleared.
        /// </summary>
        public IGridRow NewRow { get; }
    }
}
