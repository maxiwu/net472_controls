using System.Windows;
using CustomDataGrid.Models;

namespace CustomDataGrid.Contracts.Events
{
    /// <summary>
    /// Routed event data raised by the grid when a group is collapsed.
    /// </summary>
    public class GroupCollapsedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCollapsedEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event being raised.</param>
        /// <param name="group">The group that was collapsed.</param>
        /// <param name="groupIndex">The zero-based index of the group.</param>
        public GroupCollapsedEventArgs(RoutedEvent routedEvent, GridGroupRow group, int groupIndex)
            : base(routedEvent)
        {
            Group = group;
            GroupIndex = groupIndex;
        }

        /// <summary>
        /// Gets the group that was collapsed.
        /// </summary>
        public GridGroupRow Group { get; }

        /// <summary>
        /// Gets the zero-based index of the group within the data source.
        /// </summary>
        public int GroupIndex { get; }
    }
}
