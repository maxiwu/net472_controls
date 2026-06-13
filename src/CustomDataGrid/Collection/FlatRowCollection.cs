using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using CustomDataGrid.Contracts;
using CustomDataGrid.Contracts.Events;
using CustomDataGrid.Models;

namespace CustomDataGrid.Collection
{
    /// <summary>
    /// A virtual <see cref="IList{T}"/> of <see cref="IGridRow"/> that flattens a
    /// two-level hierarchy (groups containing items) into a single indexed sequence
    /// without holding any item objects in memory.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Row objects are resolved on demand from the <see cref="IGridDataSource"/>
    /// supplied at construction time. Only rows in the current viewport are ever
    /// materialized; all others are discarded between calls.
    /// </para>
    /// <para>
    /// The indexer runs in <b>O(log G)</b> time (binary search over per-group flat
    /// offsets, where G is the number of groups).
    /// </para>
    /// <para>
    /// Expand and collapse operations fire incremental
    /// <see cref="NotifyCollectionChangedAction.Add"/> /
    /// <see cref="NotifyCollectionChangedAction.Remove"/>
    /// <see cref="CollectionChanged"/> notifications — never
    /// <see cref="NotifyCollectionChangedAction.Reset"/> — so
    /// <c>VirtualizingStackPanel</c> can keep recycling its containers.
    /// </para>
    /// <para>
    /// <b>Warning (CollectionView trap):</b> setting this collection as
    /// <c>ItemsControl.ItemsSource</c> causes WPF to wrap it in a default
    /// <c>CollectionView</c>. Applying sort descriptions, a filter delegate, or
    /// group descriptions on that view will enumerate the <em>entire</em>
    /// collection, forcing <see cref="IGridDataSource.GetItem"/> to be called for
    /// every row and defeating data virtualization. Keep the default view free of
    /// any sort / filter / group configuration.
    /// </para>
    /// <para>
    /// <b>Why this class also implements non-generic <see cref="IList"/>:</b> WPF
    /// only selects the non-enumerating <c>ListCollectionView</c> for sources that
    /// implement non-generic <see cref="IList"/>. A source that implements only
    /// <see cref="IList{T}"/> is wrapped in <c>EnumerableCollectionView</c>, which
    /// eagerly enumerates the entire source once on construction — calling
    /// <see cref="IGridDataSource.GetGroup"/> for every group up front and
    /// defeating data virtualization before any layout even happens.
    /// </para>
    /// <para>
    /// <b>Why this class also implements <see cref="ICollectionViewFactory"/> /
    /// <see cref="ICollectionView"/>:</b> even with non-generic <see cref="IList"/>,
    /// <c>ListCollectionView</c> still builds an internal snapshot of the entire
    /// source on construction (an O(n) enumeration). Implementing
    /// <see cref="ICollectionViewFactory"/> lets this class hand WPF a view over
    /// itself — <see cref="ICollectionViewFactory.CreateView"/> returns
    /// <c>this</c> — with no sorting, filtering, grouping, or currency tracking,
    /// so <c>ItemsControl.ItemsSource</c> never wraps it in a snapshot-copying
    /// view at all.
    /// </para>
    /// </remarks>
    public class FlatRowCollection : IList<IGridRow>, IList, INotifyCollectionChanged, ICollectionViewFactory, ICollectionView
    {
        private readonly List<GroupState> _groupStates = new List<GroupState>();
        private IGridDataSource _source;
        private bool _singleExpandMode;
        private int _currentExpandedGroupIndex = -1;

        private struct GroupState
        {
            /// <summary>Index of the group header row in the flat list.</summary>
            public int FlatOffset;
            /// <summary>Whether this group's items are currently visible.</summary>
            public bool IsExpanded;
            /// <summary>Number of item rows currently inserted into the flat list for this group.</summary>
            public int LoadedItemCount;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FlatRowCollection"/> backed by
        /// the supplied data source.
        /// </summary>
        /// <param name="source">
        /// The data source to resolve groups and items from. Must not be <c>null</c>.
        /// </param>
        public FlatRowCollection(IGridDataSource source)
        {
            if (source == null) throw new ArgumentNullException("source");
            _source = source;
            SubscribeTo(source);
            Rebuild();
        }

        // ------------------------------------------------------------------ //
        //  Public API                                                          //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Gets or sets whether only one group may be expanded at a time.
        /// Setting to <c>true</c> immediately collapses all but the first
        /// currently-expanded group.
        /// </summary>
        public bool SingleExpandMode
        {
            get { return _singleExpandMode; }
            set
            {
                if (_singleExpandMode == value) return;
                _singleExpandMode = value;
                if (value) EnforceSingleExpandMode();
            }
        }

        /// <summary>
        /// Expands or collapses the group at <paramref name="groupIndex"/>.
        /// </summary>
        /// <remarks>
        /// When <see cref="SingleExpandMode"/> is <c>true</c> and
        /// <paramref name="expanded"/> is <c>true</c>, the previously-expanded
        /// group is collapsed first and
        /// <see cref="IGridCollapseHint.OnGroupCollapsed"/> is called if the
        /// data source opts in.
        /// <para>
        /// No-op if the group is disabled (see
        /// <see cref="GridGroupRow.IsEnabled"/>).
        /// </para>
        /// </remarks>
        public void SetExpanded(int groupIndex, bool expanded)
        {
            if (expanded)
            {
                if (_singleExpandMode)
                {
                    if (_currentExpandedGroupIndex >= 0 && _currentExpandedGroupIndex != groupIndex)
                    {
                        int prev = _currentExpandedGroupIndex;
                        CollapseGroup(prev);
                        _currentExpandedGroupIndex = -1;
                        var hint = _source as IGridCollapseHint;
                        if (hint != null) hint.OnGroupCollapsed(prev);
                    }
                }
                ExpandGroup(groupIndex);
            }
            else
            {
                CollapseGroup(groupIndex);
                if (_currentExpandedGroupIndex == groupIndex)
                    _currentExpandedGroupIndex = -1;
            }
        }

        /// <summary>
        /// Collapses all groups except the first one that is currently expanded,
        /// satisfying the single-expand invariant after
        /// <see cref="SingleExpandMode"/> is toggled to <c>true</c>.
        /// </summary>
        public void EnforceSingleExpandMode()
        {
            int firstExpanded = -1;
            for (int g = 0; g < _groupStates.Count; g++)
            {
                if (_groupStates[g].IsExpanded)
                {
                    if (firstExpanded == -1)
                    {
                        firstExpanded = g;
                    }
                    else
                    {
                        CollapseGroup(g);
                        // CollapseGroup sets IsExpanded=false at g, so the loop
                        // correctly continues to the next index.
                    }
                }
            }
            _currentExpandedGroupIndex = firstExpanded;
        }

        // ------------------------------------------------------------------ //
        //  INotifyCollectionChanged                                           //
        // ------------------------------------------------------------------ //

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        // ------------------------------------------------------------------ //
        //  IList<IGridRow> — read path                                        //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Gets the total number of rows currently visible (all group headers
        /// plus the item rows of any expanded groups).
        /// </summary>
        public int Count
        {
            get
            {
                if (_groupStates.Count == 0) return 0;
                var last = _groupStates[_groupStates.Count - 1];
                return last.FlatOffset + 1 + last.LoadedItemCount;
            }
        }

        /// <summary>
        /// Gets the row at <paramref name="index"/> in the flat list.
        /// </summary>
        /// <remarks>
        /// Runs in O(log G) time via binary search over
        /// <see cref="GroupState.FlatOffset"/> values.
        /// </remarks>
        public IGridRow this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException("index");

                // Binary search: find the group with the largest FlatOffset <= index.
                // Ceiling-mid variant avoids infinite loop when hi = lo + 1.
                int lo = 0, hi = _groupStates.Count - 1;
                while (lo < hi)
                {
                    int mid = lo + (hi - lo + 1) / 2;
                    if (_groupStates[mid].FlatOffset <= index)
                        lo = mid;
                    else
                        hi = mid - 1;
                }

                var gs = _groupStates[lo];
                if (index == gs.FlatOffset)
                    return _source.GetGroup(lo);

                return _source.GetItem(lo, index - gs.FlatOffset - 1);
            }
            set { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        public bool IsReadOnly { get { return true; } }

        /// <inheritdoc/>
        public bool Contains(IGridRow item)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public int IndexOf(IGridRow item)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public void CopyTo(IGridRow[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
                array[arrayIndex + i] = this[i];
        }

        /// <inheritdoc/>
        public IEnumerator<IGridRow> GetEnumerator()
        {
            int count = Count;
            for (int i = 0; i < count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        // ------------------------------------------------------------------ //
        //  IList<IGridRow> — write path (not supported)                       //
        // ------------------------------------------------------------------ //

        /// <inheritdoc/>
        public void Add(IGridRow item) { throw new NotSupportedException(); }
        /// <inheritdoc/>
        public void Clear() { throw new NotSupportedException(); }
        /// <inheritdoc/>
        public void Insert(int index, IGridRow item) { throw new NotSupportedException(); }
        /// <inheritdoc/>
        public bool Remove(IGridRow item) { throw new NotSupportedException(); }
        /// <inheritdoc/>
        public void RemoveAt(int index) { throw new NotSupportedException(); }

        // ------------------------------------------------------------------ //
        //  Non-generic IList — required so WPF uses ListCollectionView        //
        //  instead of the eagerly-enumerating EnumerableCollectionView. See   //
        //  the class remarks above.                                           //
        // ------------------------------------------------------------------ //

        bool IList.IsFixedSize { get { return true; } }
        bool IList.IsReadOnly { get { return true; } }
        bool ICollection.IsSynchronized { get { return false; } }
        object ICollection.SyncRoot { get { return this; } }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { throw new NotSupportedException(); }
        }

        int IList.Add(object value) { throw new NotSupportedException(); }
        void IList.Clear() { throw new NotSupportedException(); }
        void IList.Insert(int index, object value) { throw new NotSupportedException(); }
        void IList.Remove(object value) { throw new NotSupportedException(); }
        void IList.RemoveAt(int index) { throw new NotSupportedException(); }

        bool IList.Contains(object value) { return false; }

        /// <summary>
        /// Always returns -1 without scanning. <see cref="ListCollectionView"/> calls
        /// this for currency-tracking housekeeping (e.g. resolving <c>CurrentItem</c>
        /// after a refresh); a linear scan over a 2M-row source would materialize
        /// every group via <see cref="this"/> just to answer a question WPF handles
        /// gracefully as "not found". This view is read-only and never reordered, so
        /// no caller needs a real index-of-item lookup.
        /// </summary>
        int IList.IndexOf(object value) { return -1; }

        void ICollection.CopyTo(Array array, int index)
        {
            for (int i = 0; i < Count; i++)
                array.SetValue(this[i], index + i);
        }

        // ------------------------------------------------------------------ //
        //  ICollectionViewFactory / ICollectionView                           //
        //                                                                      //
        //  Returning `this` from CreateView bypasses ListCollectionView /      //
        //  EnumerableCollectionView entirely — see the class remarks above.    //
        //  Sorting, filtering, grouping, and currency are all unsupported:     //
        //  this is a read-only, flat, virtualized row source.                  //
        // ------------------------------------------------------------------ //

        /// <inheritdoc/>
        ICollectionView ICollectionViewFactory.CreateView()
        {
            return this;
        }

        /// <inheritdoc/>
        CultureInfo ICollectionView.Culture
        {
            get { return CultureInfo.CurrentCulture; }
            set { /* not supported — no culture-sensitive operations */ }
        }

        /// <inheritdoc/>
        IEnumerable ICollectionView.SourceCollection { get { return this; } }

        /// <inheritdoc/>
        Predicate<object> ICollectionView.Filter
        {
            get { return null; }
            set { throw new NotSupportedException(); }
        }

        /// <inheritdoc/>
        bool ICollectionView.CanFilter { get { return false; } }

        /// <inheritdoc/>
        SortDescriptionCollection ICollectionView.SortDescriptions { get { return SortDescriptionCollection.Empty; } }

        /// <inheritdoc/>
        bool ICollectionView.CanSort { get { return false; } }

        /// <inheritdoc/>
        bool ICollectionView.CanGroup { get { return false; } }

        /// <inheritdoc/>
        ObservableCollection<GroupDescription> ICollectionView.GroupDescriptions { get { return null; } }

        /// <inheritdoc/>
        ReadOnlyObservableCollection<object> ICollectionView.Groups { get { return null; } }

        /// <inheritdoc/>
        bool ICollectionView.IsEmpty { get { return Count == 0; } }

        /// <inheritdoc/>
        object ICollectionView.CurrentItem { get { return null; } }

        /// <inheritdoc/>
        int ICollectionView.CurrentPosition { get { return -1; } }

        /// <inheritdoc/>
        bool ICollectionView.IsCurrentAfterLast { get { return true; } }

        /// <inheritdoc/>
        bool ICollectionView.IsCurrentBeforeFirst { get { return true; } }

        /// <inheritdoc/>
        bool ICollectionView.Contains(object item) { return false; }

        /// <inheritdoc/>
        void ICollectionView.Refresh() { /* no-op — no sort/filter/group state to recompute */ }

        /// <inheritdoc/>
        IDisposable ICollectionView.DeferRefresh() { return NullDisposable.Instance; }

        /// <inheritdoc/>
        bool ICollectionView.MoveCurrentToFirst() { return false; }

        /// <inheritdoc/>
        bool ICollectionView.MoveCurrentToLast() { return false; }

        /// <inheritdoc/>
        bool ICollectionView.MoveCurrentToNext() { return false; }

        /// <inheritdoc/>
        bool ICollectionView.MoveCurrentToPrevious() { return false; }

        /// <inheritdoc/>
        bool ICollectionView.MoveCurrentTo(object item) { return false; }

        /// <inheritdoc/>
        bool ICollectionView.MoveCurrentToPosition(int position) { return false; }

        /// <inheritdoc/>
        event EventHandler ICollectionView.CurrentChanged
        {
            add { /* currency not tracked */ }
            remove { /* currency not tracked */ }
        }

        /// <inheritdoc/>
        event CurrentChangingEventHandler ICollectionView.CurrentChanging
        {
            add { /* currency not tracked */ }
            remove { /* currency not tracked */ }
        }

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new NullDisposable();
            public void Dispose() { }
        }

        // ------------------------------------------------------------------ //
        //  Internal — expand / collapse                                       //
        // ------------------------------------------------------------------ //

        private void ExpandGroup(int groupIndex)
        {
            var gs = _groupStates[groupIndex];
            if (gs.IsExpanded) return;

            var group = _source.GetGroup(groupIndex);
            if (!group.IsEnabled) return; // disabled group cannot expand

            int itemCount = _source.GetItemCount(groupIndex);
            int insertAt = gs.FlatOffset + 1;

            gs.IsExpanded = true;
            gs.LoadedItemCount = itemCount;
            _groupStates[groupIndex] = gs;
            group.IsExpanded = true;
            _currentExpandedGroupIndex = groupIndex;

            AdjustOffsets(groupIndex + 1, itemCount);

            if (itemCount > 0)
            {
                var items = new LazyItemList(_source, groupIndex, 0, itemCount);
                FireAdd(items, insertAt);
            }
        }

        private void CollapseGroup(int groupIndex)
        {
            var gs = _groupStates[groupIndex];
            if (!gs.IsExpanded) return;

            int removeAt = gs.FlatOffset + 1;
            int removeCount = gs.LoadedItemCount;

            // Capture source items before mutating state (source still has them).
            IList removeItems = removeCount > 0
                ? (IList)new LazyItemList(_source, groupIndex, 0, removeCount)
                : new List<IGridRow>();

            var group = _source.GetGroup(groupIndex);
            group.IsExpanded = false;

            gs.IsExpanded = false;
            gs.LoadedItemCount = 0;
            _groupStates[groupIndex] = gs;

            AdjustOffsets(groupIndex + 1, -removeCount);

            if (removeCount > 0)
                FireRemove(removeItems, removeAt);
        }

        private void AdjustOffsets(int fromGroupIndex, int delta)
        {
            for (int i = fromGroupIndex; i < _groupStates.Count; i++)
            {
                var gs = _groupStates[i];
                gs.FlatOffset += delta;
                _groupStates[i] = gs;
            }
        }

        // ------------------------------------------------------------------ //
        //  Internal — rebuild                                                 //
        // ------------------------------------------------------------------ //

        private void Rebuild()
        {
            _groupStates.Clear();
            _currentExpandedGroupIndex = -1;
            int offset = 0;
            for (int g = 0; g < _source.GroupCount; g++)
            {
                _groupStates.Add(new GroupState
                {
                    FlatOffset = offset,
                    IsExpanded = false,
                    LoadedItemCount = 0
                });
                offset += 1; // group header only (all collapsed)
            }
        }

        // ------------------------------------------------------------------ //
        //  Internal — data source event handlers                              //
        // ------------------------------------------------------------------ //

        private void SubscribeTo(IGridDataSource source)
        {
            source.GroupChanged += OnGroupChanged;
            source.ItemChanged += OnItemChanged;
            source.DataReset += OnDataReset;
        }

        private void OnDataReset(object sender, EventArgs e)
        {
            Rebuild();
            var handler = CollectionChanged;
            if (handler != null)
                handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnGroupChanged(object sender, GroupChangedEventArgs e)
        {
            switch (e.Kind)
            {
                case GroupChangeKind.Added:
                    HandleGroupAdded(e.GroupIndex);
                    break;
                case GroupChangeKind.Removed:
                    HandleGroupRemoved(e.GroupIndex);
                    break;
                case GroupChangeKind.Updated:
                    HandleGroupUpdated(e.GroupIndex);
                    break;
            }
        }

        private void HandleGroupAdded(int groupIndex)
        {
            int flatOffset = groupIndex == 0
                ? 0
                : _groupStates[groupIndex - 1].FlatOffset + 1 + _groupStates[groupIndex - 1].LoadedItemCount;

            _groupStates.Insert(groupIndex, new GroupState
            {
                FlatOffset = flatOffset,
                IsExpanded = false,
                LoadedItemCount = 0
            });

            AdjustOffsets(groupIndex + 1, 1);

            if (_currentExpandedGroupIndex >= groupIndex)
                _currentExpandedGroupIndex++;

            FireAdd(new List<IGridRow> { _source.GetGroup(groupIndex) }, flatOffset);
        }

        private void HandleGroupRemoved(int groupIndex)
        {
            var gs = _groupStates[groupIndex];
            int removeAt = gs.FlatOffset;
            // Remove the header + any loaded items in a single notification.
            int removeCount = 1 + gs.LoadedItemCount;

            if (_currentExpandedGroupIndex == groupIndex)
                _currentExpandedGroupIndex = -1;
            else if (_currentExpandedGroupIndex > groupIndex)
                _currentExpandedGroupIndex--;

            _groupStates.RemoveAt(groupIndex);
            AdjustOffsets(groupIndex, -removeCount);

            // Source has already removed the group; pass nulls as placeholder items.
            FireRemove(NullList(removeCount), removeAt);
        }

        private void HandleGroupUpdated(int groupIndex)
        {
            int flatOffset = _groupStates[groupIndex].FlatOffset;
            var group = _source.GetGroup(groupIndex);
            FireReplace(group, group, flatOffset);
        }

        private void OnItemChanged(object sender, ItemChangedEventArgs e)
        {
            var gs = _groupStates[e.GroupIndex];
            if (!gs.IsExpanded) return; // group collapsed — no flat change needed

            int flatItemAt = gs.FlatOffset + 1 + e.ItemIndex;

            switch (e.Kind)
            {
                case ItemChangeKind.Added:
                {
                    var newGs = gs;
                    newGs.LoadedItemCount++;
                    _groupStates[e.GroupIndex] = newGs;
                    AdjustOffsets(e.GroupIndex + 1, 1);
                    var item = _source.GetItem(e.GroupIndex, e.ItemIndex);
                    FireAdd(new List<IGridRow> { item }, flatItemAt);
                    break;
                }
                case ItemChangeKind.Removed:
                {
                    var newGs = gs;
                    newGs.LoadedItemCount--;
                    _groupStates[e.GroupIndex] = newGs;
                    AdjustOffsets(e.GroupIndex + 1, -1);
                    // Item already removed from source; pass null placeholder.
                    FireRemove(NullList(1), flatItemAt);
                    break;
                }
                case ItemChangeKind.Updated:
                {
                    var item = _source.GetItem(e.GroupIndex, e.ItemIndex);
                    FireReplace(item, item, flatItemAt);
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ //
        //  Internal — CollectionChanged helpers                               //
        // ------------------------------------------------------------------ //

        private void FireAdd(IList newItems, int startIndex)
        {
            var handler = CollectionChanged;
            if (handler == null) return;
            handler(this, new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add, newItems, startIndex));
        }

        private void FireRemove(IList oldItems, int startIndex)
        {
            var handler = CollectionChanged;
            if (handler == null) return;
            handler(this, new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove, oldItems, startIndex));
        }

        private void FireReplace(IGridRow newItem, IGridRow oldItem, int index)
        {
            var handler = CollectionChanged;
            if (handler == null) return;
            handler(this, new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }

        private static IList NullList(int count)
        {
            var list = new List<IGridRow>(count);
            for (int i = 0; i < count; i++) list.Add(null);
            return list;
        }

        // ------------------------------------------------------------------ //
        //  LazyItemList — defers item fetching until WPF requests each row   //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// A non-generic <see cref="IList"/> whose elements are fetched from
        /// <see cref="IGridDataSource"/> on demand. Passed to ranged
        /// <see cref="NotifyCollectionChangedEventArgs"/> so that WPF's
        /// <c>VirtualizingStackPanel</c> only fetches the rows it actually needs
        /// to render, not all rows in the group.
        /// </summary>
        private sealed class LazyItemList : IList
        {
            private readonly IGridDataSource _source;
            private readonly int _groupIndex;
            private readonly int _startItemIndex;
            private readonly int _count;

            internal LazyItemList(IGridDataSource source, int groupIndex, int startItemIndex, int count)
            {
                _source = source;
                _groupIndex = groupIndex;
                _startItemIndex = startItemIndex;
                _count = count;
            }

            public object this[int index]
            {
                get { return _source.GetItem(_groupIndex, _startItemIndex + index); }
                set { throw new NotSupportedException(); }
            }

            public int Count { get { return _count; } }
            public bool IsReadOnly { get { return true; } }
            public bool IsFixedSize { get { return true; } }
            public bool IsSynchronized { get { return false; } }
            public object SyncRoot { get { return this; } }

            public IEnumerator GetEnumerator()
            {
                for (int i = 0; i < _count; i++) yield return this[i];
            }

            public bool Contains(object value) { throw new NotSupportedException(); }
            public int IndexOf(object value) { throw new NotSupportedException(); }
            public void CopyTo(Array array, int index)
            {
                for (int i = 0; i < _count; i++)
                    array.SetValue(this[i], index + i);
            }
            public int Add(object value) { throw new NotSupportedException(); }
            public void Clear() { throw new NotSupportedException(); }
            public void Insert(int index, object value) { throw new NotSupportedException(); }
            public void Remove(object value) { throw new NotSupportedException(); }
            public void RemoveAt(int index) { throw new NotSupportedException(); }
        }
    }
}
