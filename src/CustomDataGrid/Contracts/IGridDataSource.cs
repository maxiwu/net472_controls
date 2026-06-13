using System;
using System.Collections.Generic;
using CustomDataGrid.Contracts.Events;
using CustomDataGrid.Models;

namespace CustomDataGrid.Contracts
{
    /// <summary>
    /// The data-access contract the grid talks to. The control knows nothing
    /// about where the data lives; an implementation may be backed by an
    /// in-memory list, a database, or a remote service.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All count values (<see cref="GroupCount"/> and <see cref="GetItemCount"/>)
    /// must be known up front so the control can size the scrollbar correctly
    /// without loading every row.
    /// </para>
    /// <para>
    /// Row objects are fetched on demand through the <c>Get*</c> methods. The
    /// control only requests rows that are about to become visible, which is
    /// what makes large data sets (up to millions of rows) feasible.
    /// </para>
    /// <para>
    /// When the underlying data changes, the implementation raises
    /// <see cref="GroupChanged"/>, <see cref="ItemChanged"/>, or
    /// <see cref="DataReset"/> so the control can update incrementally.
    /// </para>
    /// <para>
    /// An implementation may additionally implement <see cref="IGridCollapseHint"/>
    /// to be told when a group is collapsed, giving paged sources an opportunity
    /// to release cached pages.
    /// </para>
    /// </remarks>
    public interface IGridDataSource
    {
        /// <summary>
        /// Gets the total number of groups. Must be known without loading items.
        /// </summary>
        int GroupCount { get; }

        /// <summary>
        /// Gets the number of items in the specified group. Must be known without
        /// loading the items themselves.
        /// </summary>
        /// <param name="groupIndex">The zero-based group index.</param>
        /// <returns>The item count for that group.</returns>
        int GetItemCount(int groupIndex);

        /// <summary>
        /// Gets the group at the specified index. Called on demand for visible rows.
        /// </summary>
        /// <param name="groupIndex">The zero-based group index.</param>
        /// <returns>The group row.</returns>
        GridGroupRow GetGroup(int groupIndex);

        /// <summary>
        /// Gets a single item from a group. Called on demand for visible rows.
        /// </summary>
        /// <param name="groupIndex">The zero-based group index.</param>
        /// <param name="itemIndex">The zero-based item index within the group.</param>
        /// <returns>The item row.</returns>
        GridItemRow GetItem(int groupIndex, int itemIndex);

        /// <summary>
        /// Gets a contiguous range of items from a group in a single call. Allows
        /// implementations to batch-load (e.g. fetch a page) more efficiently than
        /// repeated <see cref="GetItem"/> calls.
        /// </summary>
        /// <param name="groupIndex">The zero-based group index.</param>
        /// <param name="startIndex">The zero-based index of the first item to fetch.</param>
        /// <param name="count">The number of items to fetch.</param>
        /// <returns>
        /// A list of the requested items, in order. The list length should equal
        /// <paramref name="count"/> unless the range exceeds the group's bounds.
        /// </returns>
        IList<GridItemRow> GetItems(int groupIndex, int startIndex, int count);

        /// <summary>
        /// Raised when a group is added, removed, or updated in the data source.
        /// </summary>
        event EventHandler<GroupChangedEventArgs> GroupChanged;

        /// <summary>
        /// Raised when an item is added, removed, or updated in the data source.
        /// </summary>
        event EventHandler<ItemChangedEventArgs> ItemChanged;

        /// <summary>
        /// Raised when the entire data set changes and the control should reload
        /// from scratch.
        /// </summary>
        event EventHandler DataReset;
    }
}
