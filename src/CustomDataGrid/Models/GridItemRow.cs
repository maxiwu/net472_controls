using System.ComponentModel;
using System.Runtime.CompilerServices;
using CustomDataGrid.Contracts;

namespace CustomDataGrid.Models
{
    /// <summary>
    /// A data item row belonging to a <see cref="GridGroupRow"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <see cref="IsEnabled"/> is <c>false</c>, the row cannot be selected,
    /// highlighted, or edited (see the rules on <see cref="IGridRow"/>). Because
    /// edit mode is unreachable in that state, the control's
    /// <c>CellEditCommitted</c> and <c>CellEditCancelled</c> events are
    /// unreachable for disabled rows too.
    /// </para>
    /// <para>
    /// An item that belongs to a disabled group is treated as disabled by the
    /// control regardless of this row's own <see cref="IsEnabled"/> value.
    /// </para>
    /// </remarks>
    public class GridItemRow : IGridRow, INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isEnabled = true;
        private bool _isHighlighted;

        /// <summary>
        /// Gets <see cref="RowKind.Item"/>.
        /// </summary>
        public RowKind Kind
        {
            get { return RowKind.Item; }
        }

        /// <summary>
        /// Gets or sets whether the row is selected. Has no effect when
        /// <see cref="IsEnabled"/> is <c>false</c>; the control's
        /// <c>SelectRowCommand</c> ignores disabled rows.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether the row can be interacted with. See the remarks
        /// on <see cref="GridItemRow"/> for the full set of consequences.
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
        /// Gets or sets whether the row is rendered with the highlight color.
        /// A consumer may bind this to the same backing value as
        /// <see cref="IsSelected"/> so that selecting a row also highlights it.
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
