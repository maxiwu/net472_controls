using System.Windows;
using CustomDataGrid.Models;

namespace CustomDataGrid.Contracts.Events
{
    /// <summary>
    /// Routed event data raised by the grid when a group is expanded.
    /// </summary>
    public class GroupExpandedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupExpandedEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event being raised.</param>
        /// <param name="group">The group that was expanded.</param>
        /// <param name="groupIndex">The zero-based index of the group.</param>
        public GroupExpandedEventArgs(RoutedEvent routedEvent, GridGroupRow group, int groupIndex)
            : base(routedEvent)
        {
            Group = group;
            GroupIndex = groupIndex;
        }

        /// <summary>
        /// Gets the group that was expanded.
        /// </summary>
        public GridGroupRow Group { get; }

        /// <summary>
        /// Gets the zero-based index of the group within the data source.
        /// </summary>
        public int GroupIndex { get; }
    }
}
