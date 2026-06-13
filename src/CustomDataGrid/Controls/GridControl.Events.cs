using System.Collections.Generic;
using System.Windows;
using CustomDataGrid.Contracts;
using CustomDataGrid.Contracts.Events;
using CustomDataGrid.Models;

namespace CustomDataGrid.Controls
{
    /// <summary>
    /// Outbound routed events (Task 5.3). All six events bubble up the visual
    /// tree (<see cref="RoutingStrategy.Bubble"/>) so a host window can handle
    /// them at any ancestor without attaching to the grid directly.
    /// </summary>
    public partial class GridControl
    {
        /// <summary>
        /// Identifies the <see cref="SelectedRowChanged"/> routed event.
        /// </summary>
        public static readonly RoutedEvent SelectedRowChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SelectedRowChanged),
            RoutingStrategy.Bubble,
            typeof(System.Windows.RoutedEventHandler),
            typeof(GridControl));

        /// <summary>
        /// Identifies the <see cref="SelectedRowsChanged"/> routed event.
        /// </summary>
        public static readonly RoutedEvent SelectedRowsChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SelectedRowsChanged),
            RoutingStrategy.Bubble,
            typeof(System.Windows.RoutedEventHandler),
            typeof(GridControl));

        /// <summary>
        /// Identifies the <see cref="GroupExpanded"/> routed event.
        /// </summary>
        public static readonly RoutedEvent GroupExpandedEvent = EventManager.RegisterRoutedEvent(
            nameof(GroupExpanded),
            RoutingStrategy.Bubble,
            typeof(System.Windows.RoutedEventHandler),
            typeof(GridControl));

        /// <summary>
        /// Identifies the <see cref="GroupCollapsed"/> routed event.
        /// </summary>
        public static readonly RoutedEvent GroupCollapsedEvent = EventManager.RegisterRoutedEvent(
            nameof(GroupCollapsed),
            RoutingStrategy.Bubble,
            typeof(System.Windows.RoutedEventHandler),
            typeof(GridControl));

        /// <summary>
        /// Identifies the <see cref="CellEditCommitted"/> routed event.
        /// </summary>
        public static readonly RoutedEvent CellEditCommittedEvent = EventManager.RegisterRoutedEvent(
            nameof(CellEditCommitted),
            RoutingStrategy.Bubble,
            typeof(System.Windows.RoutedEventHandler),
            typeof(GridControl));

        /// <summary>
        /// Identifies the <see cref="CellEditCancelled"/> routed event.
        /// </summary>
        public static readonly RoutedEvent CellEditCancelledEvent = EventManager.RegisterRoutedEvent(
            nameof(CellEditCancelled),
            RoutingStrategy.Bubble,
            typeof(System.Windows.RoutedEventHandler),
            typeof(GridControl));

        /// <summary>
        /// Raised when <see cref="SelectedRow"/> changes.
        /// </summary>
        public event RoutedEventHandler SelectedRowChanged
        {
            add { AddHandler(SelectedRowChangedEvent, value); }
            remove { RemoveHandler(SelectedRowChangedEvent, value); }
        }

        /// <summary>
        /// Raised when the contents of <see cref="SelectedRows"/> change.
        /// </summary>
        public event RoutedEventHandler SelectedRowsChanged
        {
            add { AddHandler(SelectedRowsChangedEvent, value); }
            remove { RemoveHandler(SelectedRowsChangedEvent, value); }
        }

        /// <summary>
        /// Raised when a group row is expanded.
        /// </summary>
        public event RoutedEventHandler GroupExpanded
        {
            add { AddHandler(GroupExpandedEvent, value); }
            remove { RemoveHandler(GroupExpandedEvent, value); }
        }

        /// <summary>
        /// Raised when a group row is collapsed.
        /// </summary>
        public event RoutedEventHandler GroupCollapsed
        {
            add { AddHandler(GroupCollapsedEvent, value); }
            remove { RemoveHandler(GroupCollapsedEvent, value); }
        }

        /// <summary>
        /// Raised when a cell edit is committed.
        /// </summary>
        public event RoutedEventHandler CellEditCommitted
        {
            add { AddHandler(CellEditCommittedEvent, value); }
            remove { RemoveHandler(CellEditCommittedEvent, value); }
        }

        /// <summary>
        /// Raised when a cell edit is cancelled.
        /// </summary>
        public event RoutedEventHandler CellEditCancelled
        {
            add { AddHandler(CellEditCancelledEvent, value); }
            remove { RemoveHandler(CellEditCancelledEvent, value); }
        }

        /// <summary>
        /// Raises <see cref="SelectedRowChanged"/>.
        /// </summary>
        /// <param name="oldRow">The previously selected row, or <c>null</c>.</param>
        /// <param name="newRow">The newly selected row, or <c>null</c>.</param>
        protected void RaiseSelectedRowChanged(IGridRow oldRow, IGridRow newRow)
        {
            RaiseEvent(new SelectedRowChangedEventArgs(SelectedRowChangedEvent, oldRow, newRow));
        }

        /// <summary>
        /// Raises <see cref="SelectedRowsChanged"/>.
        /// </summary>
        /// <param name="addedRows">The rows added to the selection.</param>
        /// <param name="removedRows">The rows removed from the selection.</param>
        protected void RaiseSelectedRowsChanged(IList<IGridRow> addedRows, IList<IGridRow> removedRows)
        {
            RaiseEvent(new SelectedRowsChangedEventArgs(SelectedRowsChangedEvent, addedRows, removedRows));
        }

        /// <summary>
        /// Raises <see cref="GroupExpanded"/>.
        /// </summary>
        /// <param name="group">The group that was expanded.</param>
        /// <param name="groupIndex">The zero-based index of the group.</param>
        protected void RaiseGroupExpanded(GridGroupRow group, int groupIndex)
        {
            RaiseEvent(new GroupExpandedEventArgs(GroupExpandedEvent, group, groupIndex));
        }

        /// <summary>
        /// Raises <see cref="GroupCollapsed"/>.
        /// </summary>
        /// <param name="group">The group that was collapsed.</param>
        /// <param name="groupIndex">The zero-based index of the group.</param>
        protected void RaiseGroupCollapsed(GridGroupRow group, int groupIndex)
        {
            RaiseEvent(new GroupCollapsedEventArgs(GroupCollapsedEvent, group, groupIndex));
        }

        /// <summary>
        /// Raises <see cref="CellEditCommitted"/>.
        /// </summary>
        /// <param name="row">The row whose cell was edited.</param>
        /// <param name="columnName">The name of the edited column.</param>
        /// <param name="oldValue">The value before the edit.</param>
        /// <param name="newValue">The value after the edit.</param>
        protected void RaiseCellEditCommitted(IGridRow row, string columnName, object oldValue, object newValue)
        {
            RaiseEvent(new CellEditCommittedEventArgs(CellEditCommittedEvent, row, columnName, oldValue, newValue));
        }

        /// <summary>
        /// Raises <see cref="CellEditCancelled"/>.
        /// </summary>
        /// <param name="row">The row whose cell edit was cancelled.</param>
        /// <param name="columnName">The name of the column whose edit was cancelled.</param>
        protected void RaiseCellEditCancelled(IGridRow row, string columnName)
        {
            RaiseEvent(new CellEditCancelledEventArgs(CellEditCancelledEvent, row, columnName));
        }
    }
}
