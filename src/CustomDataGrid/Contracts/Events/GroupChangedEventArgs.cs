using System;

namespace CustomDataGrid.Contracts.Events
{
    /// <summary>
    /// Carries the details of a group-level change raised by
    /// <see cref="IGridDataSource.GroupChanged"/>.
    /// </summary>
    public class GroupChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupChangedEventArgs"/> class.
        /// </summary>
        /// <param name="groupIndex">The zero-based index of the affected group.</param>
        /// <param name="kind">The kind of change that occurred.</param>
        public GroupChangedEventArgs(int groupIndex, GroupChangeKind kind)
        {
            GroupIndex = groupIndex;
            Kind = kind;
        }

        /// <summary>
        /// Gets the zero-based index of the affected group within the data source.
        /// For <see cref="GroupChangeKind.Removed"/> this is the index the group
        /// occupied immediately before removal.
        /// </summary>
        public int GroupIndex { get; }

        /// <summary>
        /// Gets the kind of change that occurred.
        /// </summary>
        public GroupChangeKind Kind { get; }
    }
}
