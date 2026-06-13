using System;

namespace CustomDataGrid.Contracts.Events
{
    /// <summary>
    /// Carries the details of an item-level change raised by
    /// <see cref="IGridDataSource.ItemChanged"/>.
    /// </summary>
    public class ItemChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemChangedEventArgs"/> class.
        /// </summary>
        /// <param name="groupIndex">The zero-based index of the group that owns the item.</param>
        /// <param name="itemIndex">The zero-based index of the affected item within its group.</param>
        /// <param name="kind">The kind of change that occurred.</param>
        public ItemChangedEventArgs(int groupIndex, int itemIndex, ItemChangeKind kind)
        {
            GroupIndex = groupIndex;
            ItemIndex = itemIndex;
            Kind = kind;
        }

        /// <summary>
        /// Gets the zero-based index of the group that owns the affected item.
        /// </summary>
        public int GroupIndex { get; }

        /// <summary>
        /// Gets the zero-based index of the affected item within its group.
        /// For <see cref="ItemChangeKind.Removed"/> this is the index the item
        /// occupied immediately before removal.
        /// </summary>
        public int ItemIndex { get; }

        /// <summary>
        /// Gets the kind of change that occurred.
        /// </summary>
        public ItemChangeKind Kind { get; }
    }
}
