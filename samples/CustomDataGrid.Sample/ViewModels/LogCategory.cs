namespace CustomDataGrid.Sample.ViewModels
{
    /// <summary>
    /// The category of a <see cref="LogEntry"/>, used to color-code the event log
    /// (Task 8.5). SELECTION = blue, EDIT = orange, ACTION = red.
    /// </summary>
    public enum LogCategory
    {
        /// <summary>Selection-related events (row / multi-row selection changes).</summary>
        Selection,

        /// <summary>Cell edit commit / cancel events.</summary>
        Edit,

        /// <summary>Action events (expand / collapse, header and row actions).</summary>
        Action
    }
}
