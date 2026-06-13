using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace CustomDataGrid.Columns
{
    /// <summary>
    /// The shared collection of column definitions for a
    /// <see cref="Controls.GridControl"/>, exposed as <c>GridControl.Columns</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the single shared width source described in design doc §5.4.
    /// <c>GridColumnHeadersPresenter</c> and every realized row's
    /// <c>GridCellsPanel</c> read column instances from this collection. In
    /// addition to the standard <see cref="ObservableCollection{T}.CollectionChanged"/>
    /// event raised when columns are added or removed, this collection raises
    /// <see cref="ColumnWidthChanged"/> whenever any contained column's
    /// <see cref="GridColumn.Width"/> changes, so consumers can react to a single
    /// "any width changed" signal without subscribing to each column individually.
    /// </para>
    /// </remarks>
    public class GridColumnCollection : ObservableCollection<GridColumn>
    {
        /// <summary>
        /// Raised when any column currently in the collection has its
        /// <see cref="GridColumn.Width"/> property changed, or when the set of
        /// columns changes (a width-affecting structural change).
        /// </summary>
        public event EventHandler ColumnWidthChanged;

        /// <inheritdoc/>
        protected override void InsertItem(int index, GridColumn item)
        {
            base.InsertItem(index, item);
            Subscribe(item);
            OnColumnWidthChanged();
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            var item = this[index];
            Unsubscribe(item);
            base.RemoveItem(index);
            OnColumnWidthChanged();
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, GridColumn item)
        {
            var old = this[index];
            Unsubscribe(old);
            base.SetItem(index, item);
            Subscribe(item);
            OnColumnWidthChanged();
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            foreach (var item in Items)
                Unsubscribe(item);
            base.ClearItems();
            OnColumnWidthChanged();
        }

        private void Subscribe(GridColumn column)
        {
            if (column == null) return;
            DependencyPropertyDescriptor
                .FromProperty(GridColumn.WidthProperty, typeof(GridColumn))
                .AddValueChanged(column, OnColumnWidthValueChanged);
        }

        private void Unsubscribe(GridColumn column)
        {
            if (column == null) return;
            DependencyPropertyDescriptor
                .FromProperty(GridColumn.WidthProperty, typeof(GridColumn))
                .RemoveValueChanged(column, OnColumnWidthValueChanged);
        }

        private void OnColumnWidthValueChanged(object sender, EventArgs e)
        {
            OnColumnWidthChanged();
        }

        private void OnColumnWidthChanged()
        {
            var handler = ColumnWidthChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
