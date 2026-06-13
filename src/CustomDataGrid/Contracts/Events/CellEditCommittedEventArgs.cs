using System.Windows;

namespace CustomDataGrid.Contracts.Events
{
    /// <summary>
    /// Routed event data raised by the grid when a cell edit is committed.
    /// </summary>
    /// <remarks>
    /// The runtime types of <see cref="OldValue"/> and <see cref="NewValue"/>
    /// depend on the column that produced the edit:
    /// <list type="bullet">
    /// <item><c>TextColumn</c> — <see cref="string"/>.</item>
    /// <item><c>ComboBoxColumn</c> — <see cref="string"/> (the display text).</item>
    /// <item><c>CheckBoxColumn</c> — <see cref="bool"/>.</item>
    /// </list>
    /// Button and actions-menu columns are not editable and never raise this event.
    /// </remarks>
    public class CellEditCommittedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CellEditCommittedEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event being raised.</param>
        /// <param name="row">The row whose cell was edited.</param>
        /// <param name="columnName">The name of the edited column.</param>
        /// <param name="oldValue">The value before the edit.</param>
        /// <param name="newValue">The value after the edit.</param>
        public CellEditCommittedEventArgs(
            RoutedEvent routedEvent,
            IGridRow row,
            string columnName,
            object oldValue,
            object newValue)
            : base(routedEvent)
        {
            Row = row;
            ColumnName = columnName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the row whose cell was edited.
        /// </summary>
        public IGridRow Row { get; }

        /// <summary>
        /// Gets the name of the edited column.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Gets the value before the edit. See the remarks on
        /// <see cref="CellEditCommittedEventArgs"/> for the runtime type per column.
        /// </summary>
        public object OldValue { get; }

        /// <summary>
        /// Gets the value after the edit. See the remarks on
        /// <see cref="CellEditCommittedEventArgs"/> for the runtime type per column.
        /// </summary>
        public object NewValue { get; }
    }
}
