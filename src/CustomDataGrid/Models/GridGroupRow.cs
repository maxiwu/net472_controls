using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CustomDataGrid.Contracts;

namespace CustomDataGrid.Models
{
    /// <summary>
    /// A group header row. Each group contains zero or more
    /// <see cref="GridItemRow"/> children, accessed through <see cref="Items"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <see cref="IsEnabled"/> is <c>false</c>, the group cannot be
    /// expanded or collapsed and all of its child rows are treated as disabled
    /// regardless of their own <see cref="GridItemRow.IsEnabled"/> value (see
    /// the rules on <see cref="IGridRow"/>).
    /// </para>
    /// <para>
    /// <see cref="SelectionState"/> is tri-state: <see cref="SelectionState.FullySelected"/>
    /// when every enabled child is selected, <see cref="SelectionState.PartiallySelected"/>
    /// when some are, <see cref="SelectionState.Deselected"/> when none are. The
    /// transitions are driven by the control, not by this model.
    /// </para>
    /// <para>
    /// <see cref="Items"/> is deliberately an <see cref="IList{T}"/> backed by a
    /// plain <see cref="List{T}"/> and not an <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/>
    /// — at the scale this control is designed for, change notifications flow
    /// through <see cref="IGridDataSource.GroupChanged"/> and
    /// <see cref="IGridDataSource.ItemChanged"/> instead of per-collection events.
    /// </para>
    /// </remarks>
    public class GridGroupRow : IGridRow, INotifyPropertyChanged
    {
        private string _label;
        private int _totalItemCount;
        private bool _isExpanded;
        private SelectionState _selectionState;
        private bool _isEnabled = true;
        private bool _isHighlighted;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridGroupRow"/> class
        /// with an empty <see cref="Items"/> list.
        /// </summary>
        public GridGroupRow()
        {
            Items = new List<GridItemRow>();
        }

        /// <summary>
        /// Gets <see cref="RowKind.Group"/>.
        /// </summary>
        public RowKind Kind
        {
            get { return RowKind.Group; }
        }

        /// <summary>
        /// Gets or sets the text shown in the group header row.
        /// </summary>
        public string Label
        {
            get { return _label; }
            set
            {
                if (_label == value) return;
                _label = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the total number of items in the group. May be larger
        /// than <see cref="Items"/>.Count when the data source pages items in
        /// on demand.
        /// </summary>
        public int TotalItemCount
        {
            get { return _totalItemCount; }
            set
            {
                if (_totalItemCount == value) return;
                _totalItemCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether the group is currently expanded. The flat row
        /// collection mutates this value as part of expand / collapse; consumers
        /// can also set it to drive the initial state.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the tri-state selection of the group. Computed by the
        /// control from the selection state of the group's enabled children.
        /// </summary>
        public SelectionState SelectionState
        {
            get { return _selectionState; }
            set
            {
                if (_selectionState == value) return;
                _selectionState = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether the group can be interacted with. When
        /// <c>false</c> the group cannot expand or collapse and all of its
        /// child rows are treated as disabled.
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether the group row is rendered with the highlight color.
        /// </summary>
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                if (_isHighlighted == value) return;
                _isHighlighted = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the child item rows belonging to this group. Mutate this list
        /// through the data source so that the grid receives the corresponding
        /// <see cref="IGridDataSource.ItemChanged"/> notification.
        /// </summary>
        public IList<GridItemRow> Items { get; }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the given property name.
        /// </summary>
        /// <param name="propertyName">The property name; supplied automatically
        /// by the compiler when called from a property setter.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
