namespace CustomDataGrid.Contracts
{
    /// <summary>
    /// Describes the selection state of a group row, which aggregates the
    /// selection of its child item rows.
    /// </summary>
    /// <remarks>
    /// Item rows are selected/deselected with a simple boolean. Group rows use
    /// this tri-state because their state is derived from their children:
    /// <list type="bullet">
    /// <item><see cref="FullySelected"/> when the group and all of its enabled children are selected.</item>
    /// <item><see cref="PartiallySelected"/> when some, but not all, enabled children are selected.</item>
    /// <item><see cref="Deselected"/> when no children are selected.</item>
    /// </list>
    /// When rendered in the selection column, <see cref="FullySelected"/> shows a tick,
    /// <see cref="PartiallySelected"/> shows a solid square, and <see cref="Deselected"/> shows an empty box.
    /// </remarks>
    public enum SelectionState
    {
        /// <summary>
        /// No child rows are selected. Renders as an empty checkbox.
        /// </summary>
        Deselected = 0,

        /// <summary>
        /// Some, but not all, enabled child rows are selected. Renders as a solid square.
        /// A click in this state transitions to <see cref="FullySelected"/>.
        /// </summary>
        PartiallySelected = 1,

        /// <summary>
        /// The group and all enabled child rows are selected. Renders as a tick.
        /// A click in this state transitions to <see cref="Deselected"/>.
        /// </summary>
        FullySelected = 2
    }
}
