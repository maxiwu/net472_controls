namespace CustomDataGrid.Contracts
{
    /// <summary>
    /// Identifies whether a row in the grid is a group header or a data item.
    /// </summary>
    public enum RowKind
    {
        /// <summary>
        /// A group header row. Contains child item rows and can be expanded or collapsed.
        /// </summary>
        Group = 0,

        /// <summary>
        /// A data item row. Always belongs to a group.
        /// </summary>
        Item = 1
    }
}
