using System;
using System.Collections.Generic;
using System.Linq;
using CustomDataGrid.Contracts;
using CustomDataGrid.Contracts.Events;
using CustomDataGrid.Models;

namespace CustomDataGrid.DataSources
{
    /// <summary>
    /// Default in-memory implementation of <see cref="IGridDataSource"/>, backed
    /// by a flat list of <see cref="GridGroupRow"/> instances (each carrying its
    /// own <see cref="GridGroupRow.Items"/>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Because <see cref="GridGroupRow.Items"/> is a plain <see cref="IList{T}"/>
    /// (not observable), all mutations must go through this class's
    /// <see cref="AddGroup"/>, <see cref="RemoveGroup"/>, <see cref="UpdateGroup"/>,
    /// <see cref="AddItem"/>, <see cref="RemoveItem"/>, <see cref="UpdateItem"/>,
    /// and <see cref="ReplaceGroups"/> methods so that the corresponding
    /// <see cref="GroupChanged"/>, <see cref="ItemChanged"/>, or
    /// <see cref="DataReset"/> notification is raised.
    /// </para>
    /// </remarks>
    public class InMemoryGridDataSource : IGridDataSource
    {
        private List<GridGroupRow> _groups;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryGridDataSource"/>
        /// class with the supplied groups.
        /// </summary>
        /// <param name="groups">The initial set of groups. Must not be <c>null</c>.</param>
        public InMemoryGridDataSource(IList<GridGroupRow> groups)
        {
            if (groups == null) throw new ArgumentNullException("groups");
            _groups = new List<GridGroupRow>(groups);
        }

        /// <inheritdoc/>
        public int GroupCount
        {
            get { return _groups.Count; }
        }

        /// <inheritdoc/>
        public int GetItemCount(int groupIndex)
        {
            return _groups[groupIndex].Items.Count;
        }

        /// <inheritdoc/>
        public GridGroupRow GetGroup(int groupIndex)
        {
            return _groups[groupIndex];
        }

        /// <inheritdoc/>
        public GridItemRow GetItem(int groupIndex, int itemIndex)
        {
            return _groups[groupIndex].Items[itemIndex];
        }

        /// <inheritdoc/>
        public IList<GridItemRow> GetItems(int groupIndex, int startIndex, int count)
        {
            var items = _groups[groupIndex].Items;
            int available = Math.Max(0, Math.Min(count, items.Count - startIndex));
            return items.Skip(startIndex).Take(available).ToList();
        }

        /// <summary>
        /// Inserts a new group at <paramref name="groupIndex"/> and raises
        /// <see cref="GroupChanged"/> with <see cref="GroupChangeKind.Added"/>.
        /// </summary>
        /// <param name="groupIndex">The zero-based index at which to insert the group.</param>
        /// <param name="group">The group to insert. Must not be <c>null</c>.</param>
        public void AddGroup(int groupIndex, GridGroupRow group)
        {
            if (group == null) throw new ArgumentNullException("group");
            _groups.Insert(groupIndex, group);
            RaiseGroupChanged(groupIndex, GroupChangeKind.Added);
        }

        /// <summary>
        /// Removes the group at <paramref name="groupIndex"/> and raises
        /// <see cref="GroupChanged"/> with <see cref="GroupChangeKind.Removed"/>.
        /// </summary>
        /// <param name="groupIndex">The zero-based index of the group to remove.</param>
        public void RemoveGroup(int groupIndex)
        {
            _groups.RemoveAt(groupIndex);
            RaiseGroupChanged(groupIndex, GroupChangeKind.Removed);
        }

        /// <summary>
        /// Raises <see cref="GroupChanged"/> with <see cref="GroupChangeKind.Updated"/>
        /// for the group at <paramref name="groupIndex"/>, signalling that its
        /// properties (e.g. label) changed in place.
        /// </summary>
        /// <param name="groupIndex">The zero-based index of the updated group.</param>
        public void UpdateGroup(int groupIndex)
        {
            RaiseGroupChanged(groupIndex, GroupChangeKind.Updated);
        }

        /// <summary>
        /// Inserts a new item at <paramref name="itemIndex"/> within the group at
        /// <paramref name="groupIndex"/> and raises <see cref="ItemChanged"/> with
        /// <see cref="ItemChangeKind.Added"/>.
        /// </summary>
        /// <param name="groupIndex">The zero-based index of the owning group.</param>
        /// <param name="itemIndex">The zero-based index at which to insert the item.</param>
        /// <param name="item">The item to insert. Must not be <c>null</c>.</param>
        public void AddItem(int groupIndex, int itemIndex, GridItemRow item)
        {
            if (item == null) throw new ArgumentNullException("item");
            _groups[groupIndex].Items.Insert(itemIndex, item);
            RaiseItemChanged(groupIndex, itemIndex, ItemChangeKind.Added);
        }

        /// <summary>
        /// Removes the item at <paramref name="itemIndex"/> within the group at
        /// <paramref name="groupIndex"/> and raises <see cref="ItemChanged"/> with
        /// <see cref="ItemChangeKind.Removed"/>.
        /// </summary>
        /// <param name="groupIndex">The zero-based index of the owning group.</param>
        /// <param name="itemIndex">The zero-based index of the item to remove.</param>
        public void RemoveItem(int groupIndex, int itemIndex)
        {
            _groups[groupIndex].Items.RemoveAt(itemIndex);
            RaiseItemChanged(groupIndex, itemIndex, ItemChangeKind.Removed);
        }

        /// <summary>
        /// Raises <see cref="ItemChanged"/> with <see cref="ItemChangeKind.Updated"/>
        /// for the item at <paramref name="itemIndex"/> within the group at
        /// <paramref name="groupIndex"/>, signalling that its properties (e.g. a
        /// cell value) changed in place.
        /// </summary>
        /// <param name="groupIndex">The zero-based index of the owning group.</param>
        /// <param name="itemIndex">The zero-based index of the updated item.</param>
        public void UpdateItem(int groupIndex, int itemIndex)
        {
            RaiseItemChanged(groupIndex, itemIndex, ItemChangeKind.Updated);
        }

        /// <summary>
        /// Replaces the entire set of groups and raises <see cref="DataReset"/>.
        /// </summary>
        /// <param name="groups">The new groups. Must not be <c>null</c>.</param>
        public void ReplaceGroups(IList<GridGroupRow> groups)
        {
            if (groups == null) throw new ArgumentNullException("groups");
            _groups = new List<GridGroupRow>(groups);

            var handler = DataReset;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public event EventHandler<GroupChangedEventArgs> GroupChanged;

        /// <inheritdoc/>
        public event EventHandler<ItemChangedEventArgs> ItemChanged;

        /// <inheritdoc/>
        public event EventHandler DataReset;

        private void RaiseGroupChanged(int groupIndex, GroupChangeKind kind)
        {
            var handler = GroupChanged;
            if (handler != null) handler(this, new GroupChangedEventArgs(groupIndex, kind));
        }

        private void RaiseItemChanged(int groupIndex, int itemIndex, ItemChangeKind kind)
        {
            var handler = ItemChanged;
            if (handler != null) handler(this, new ItemChangedEventArgs(groupIndex, itemIndex, kind));
        }
    }
}
