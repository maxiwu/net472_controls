namespace CustomDataGrid.Contracts
{
    /// <summary>
    /// The common contract shared by every row the grid displays, whether a
    /// group header (<see cref="RowKind.Group"/>) or a data item
    /// (<see cref="RowKind.Item"/>).
    /// </summary>
    /// <remarks>
    /// <para><b>Disabled rows.</b> When <see cref="IsEnabled"/> is <c>false</c>, the row:</para>
    /// <list type="bullet">
    /// <item>cannot be selected,</item>
    /// <item>cannot be highlighted,</item>
    /// <item>cannot be edited, and</item>
    /// <item>if it is a group, cannot be expanded or collapsed, and all of its
    /// child rows are treated as disabled regardless of their own
    /// <see cref="IsEnabled"/> value.</item>
    /// </list>
    /// <para>
    /// Because a disabled row can never enter edit mode, the control's
    /// <c>CellEditCommitted</c> and <c>CellEditCancelled</c> events are
    /// unreachable for disabled rows. Implementers and consumers do not need to
    /// add runtime guards for that case.
    /// </para>
    /// <para>
    /// If a row is selected and later becomes disabled, it is <i>not</i>
    /// automatically deselected. Managing that transition is the consumer's
    /// responsibility.
    /// </para>
    /// </remarks>
    public interface IGridRow
    {
        /// <summary>
        /// Gets whether this row is a group header or a data item.
        /// </summary>
        RowKind Kind { get; }

        /// <summary>
        /// Gets whether this row can be interacted with. See the remarks on
        /// <see cref="IGridRow"/> for the full set of rules that apply when this
        /// is <c>false</c>.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Gets whether this row is rendered with the control's highlight color.
        /// A consumer may bind this to the same backing value as a selection flag
        /// so that selecting a row also highlights it.
        /// </summary>
        bool IsHighlighted { get; }
    }
}
