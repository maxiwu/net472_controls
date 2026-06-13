namespace CustomDataGrid.Contracts
{
    /// <summary>
    /// An optional companion to <see cref="IGridDataSource"/>. A data source that
    /// also implements this interface is notified when the control collapses a
    /// group, giving paged or remote implementations a chance to evict cached
    /// pages and free memory.
    /// </summary>
    /// <remarks>
    /// This is a separate interface rather than a default method on
    /// <see cref="IGridDataSource"/> because the target framework (C# 7.3 /
    /// .NET Framework 4.7.2) does not support default interface methods. The
    /// control discovers support at runtime with a simple cast:
    /// <code>
    /// var hint = source as IGridCollapseHint;
    /// if (hint != null) hint.OnGroupCollapsed(groupIndex);
    /// </code>
    /// Implementing this interface is entirely optional; an in-memory source has
    /// no need for it.
    /// </remarks>
    public interface IGridCollapseHint
    {
        /// <summary>
        /// Called by the control immediately after a group has been collapsed.
        /// Implementations may use this to release any resources cached for the
        /// group's items.
        /// </summary>
        /// <param name="groupIndex">The zero-based index of the collapsed group.</param>
        void OnGroupCollapsed(int groupIndex);
    }
}
