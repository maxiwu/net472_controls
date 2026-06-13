using System;
using CustomDataGrid.Columns;
using CustomDataGrid.Contracts;

namespace CustomDataGrid.Controls
{
    /// <summary>
    /// Edit-session tracking and commit-on-click-away (Task 5.6). The control
    /// tracks at most one active edit (a row + column pair). A click anywhere
    /// outside the editing cell commits the edit before the click is processed
    /// by the selection logic in <c>GridControl.Selection.cs</c>; pressing
    /// Escape cancels it instead.
    /// </summary>
    /// <remarks>
    /// Highlight state is preserved across commit/cancel: if the row was
    /// selected before editing began, it remains selected and highlighted
    /// afterwards. Multi-select is preserved while a different row is being
    /// edited — entering edit mode never clears <see cref="GridControl.SelectedRows"/>.
    /// </remarks>
    public partial class GridControl
    {
        private IGridRow _editingRow;
        private GridColumn _editingColumn;
        private object _editingOldValue;

        /// <summary>
        /// Raised whenever the active edit cell (<see cref="EditingRow"/> /
        /// <see cref="EditingColumn"/>) changes — including when editing starts,
        /// is committed, or is cancelled. <see cref="GridCellsPanel"/> instances
        /// subscribe to this to swap a single cell between its
        /// <see cref="GridColumn.CellTemplate"/> and
        /// <see cref="GridColumn.CellEditingTemplate"/> without rebuilding every
        /// cell in the row.
        /// </summary>
        internal event EventHandler EditStateChanged;

        /// <summary>
        /// Gets the row currently being edited, or <c>null</c> if no cell is in
        /// edit mode.
        /// </summary>
        public IGridRow EditingRow
        {
            get { return _editingRow; }
        }

        /// <summary>
        /// Gets the column currently being edited, or <c>null</c> if no cell is
        /// in edit mode.
        /// </summary>
        public GridColumn EditingColumn
        {
            get { return _editingColumn; }
        }

        /// <summary>
        /// Gets whether a cell is currently in edit mode.
        /// </summary>
        public bool IsEditing
        {
            get { return _editingRow != null; }
        }

        /// <summary>
        /// Begins editing <paramref name="column"/> on <paramref name="row"/>.
        /// If a different cell is currently being edited, it is committed first.
        /// Does not change <see cref="GridControl.SelectedRows"/> or any row's
        /// highlight state. No-ops when <see cref="GridControl.IsReadOnly"/> is
        /// <c>true</c>, the row is disabled, or the column is not editable.
        /// </summary>
        /// <param name="row">The row to edit.</param>
        /// <param name="column">The column to edit.</param>
        /// <param name="currentValue">The cell's current value, recorded as the
        /// "old" value for the eventual <see cref="GridControl.CellEditCommitted"/>
        /// or <see cref="GridControl.CellEditCancelled"/> event.</param>
        public void BeginEdit(IGridRow row, GridColumn column, object currentValue)
        {
            if (IsReadOnly || row == null || column == null) return;
            if (!row.IsEnabled || !column.IsEditable) return;

            if (IsEditing && (!ReferenceEquals(_editingRow, row) || !ReferenceEquals(_editingColumn, column)))
                CommitEdit(_editingOldValue);

            _editingRow = row;
            _editingColumn = column;
            _editingOldValue = currentValue;

            OnEditStateChanged();
        }

        /// <summary>
        /// Commits the active edit, raising <see cref="GridControl.CellEditCommitted"/>
        /// with the recorded old value and <paramref name="newValue"/>. No-op if
        /// nothing is being edited.
        /// </summary>
        /// <param name="newValue">The value to commit.</param>
        public void CommitEdit(object newValue)
        {
            if (!IsEditing) return;

            var row = _editingRow;
            var column = _editingColumn;
            var oldValue = _editingOldValue;

            ClearEditState();

            RaiseCellEditCommitted(row, column.ColumnName, oldValue, newValue);
        }

        /// <summary>
        /// Cancels the active edit, raising <see cref="GridControl.CellEditCancelled"/>.
        /// No-op if nothing is being edited.
        /// </summary>
        public void CancelEdit()
        {
            if (!IsEditing) return;

            var row = _editingRow;
            var column = _editingColumn;

            ClearEditState();

            RaiseCellEditCancelled(row, column.ColumnName);
        }

        /// <summary>
        /// If <paramref name="row"/>/<paramref name="column"/> identifies a cell
        /// other than the one currently being edited, commits the active edit
        /// using <paramref name="pendingValue"/>. Intended to be called from the
        /// <see cref="System.Windows.UIElement.PreviewMouseDown"/> handler before
        /// the click is processed by the selection logic, so that clicking away
        /// from an editing cell commits it first.
        /// </summary>
        /// <param name="row">The row the click landed on, or <c>null</c> for empty space.</param>
        /// <param name="column">The column the click landed on, or <c>null</c>.</param>
        /// <param name="pendingValue">The editing cell's current (uncommitted) value.</param>
        public void CommitEditIfClickedAway(IGridRow row, GridColumn column, object pendingValue)
        {
            if (!IsEditing) return;
            if (ReferenceEquals(row, _editingRow) && ReferenceEquals(column, _editingColumn)) return;

            CommitEdit(pendingValue);
        }

        /// <summary>
        /// Raises <see cref="CellEditCommitted"/> directly for a cell that has
        /// no edit session (e.g. a <see cref="Columns.CheckBoxColumn"/>, which
        /// toggles its value in place without entering edit mode). Does not
        /// affect <see cref="EditingRow"/> / <see cref="EditingColumn"/>.
        /// </summary>
        /// <param name="row">The row whose cell value changed.</param>
        /// <param name="column">The column whose cell value changed.</param>
        /// <param name="oldValue">The value before the change.</param>
        /// <param name="newValue">The value after the change.</param>
        public void CommitCellEdit(IGridRow row, GridColumn column, object oldValue, object newValue)
        {
            RaiseCellEditCommitted(row, column.ColumnName, oldValue, newValue);
        }

        private void ClearEditState()
        {
            _editingRow = null;
            _editingColumn = null;
            _editingOldValue = null;

            OnEditStateChanged();
        }

        private void OnEditStateChanged()
        {
            var handler = EditStateChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
