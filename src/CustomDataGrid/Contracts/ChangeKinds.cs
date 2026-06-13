namespace CustomDataGrid.Contracts
{
    /// <summary>
    /// Describes the kind of change reported by
    /// <see cref="IGridDataSource.GroupChanged"/>.
    /// </summary>
    public enum GroupChangeKind
    {
        /// <summary>A new group was inserted into the data source.</summary>
        Added = 0,

        /// <summary>An existing group was removed from the data source.</summary>
        Removed = 1,

        /// <summary>An existing group's data changed in place (e.g. label edited).</summary>
        Updated = 2
    }

    /// <summary>
    /// Describes the kind of change reported by
    /// <see cref="IGridDataSource.ItemChanged"/>.
    /// </summary>
    public enum ItemChangeKind
    {
        /// <summary>A new item was inserted into a group.</summary>
        Added = 0,

        /// <summary>An existing item was removed from a group.</summary>
        Removed = 1,

        /// <summary>An existing item's data changed in place (e.g. a cell value edited).</summary>
        Updated = 2
    }
}
