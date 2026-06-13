using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CustomDataGrid.Columns;
using CustomDataGrid.Contracts;
using CustomDataGrid.Models;

namespace CustomDataGrid.Controls
{
    /// <summary>
    /// Mouse click resolution (Tasks 5.5 / 5.6). Walks the visual tree from the
    /// click point to find the <see cref="GridRowPresenter"/> and the
    /// <see cref="GridColumn"/> under the pointer, then dispatches to the
    /// selection / edit logic in <c>GridControl.Selection.cs</c> and
    /// <c>GridControl.Editing.cs</c> per the rules in design doc §5.6.
    /// </summary>
    public partial class GridControl
    {
        /// <inheritdoc/>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            var hit = HitTestRow(e.OriginalSource as DependencyObject);
            if (hit.Row == null) return;

            // Commit any in-progress edit on a different cell before processing this click.
            CommitEditIfClickedAway(hit.Row, hit.Column, null);

            if (!hit.Row.IsEnabled) return;

            var group = hit.Row as GridGroupRow;
            if (group != null)
            {
                SetGroupSelection(group);
                return;
            }

            if (hit.Column != null && hit.Column.IsEditable && !IsReadOnly)
            {
                // Editable cell: enter edit mode, do not change selection.
                BeginEdit(hit.Row, hit.Column, hit.Column.GetCellValue(hit.Row));
                return;
            }

            if (hit.Column != null && hit.Column.SuppressRowSelectionOnClick)
            {
                // CheckBox / Button / ActionsMenu / selection-column cell: the
                // cell's own handler acts; selection is unaffected here.
                return;
            }

            // Non-editable cell / empty row space: select + highlight.
            bool multiSelect = (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) != ModifierKeys.None;

            if (multiSelect)
                ToggleSelection(hit.Row);
            else
                SetSingleSelection(hit.Row);

            var item = hit.Row as GridItemRow;
            if (item != null)
            {
                var owningGroup = FindOwningGroup(item);
                if (owningGroup != null)
                    RecomputeGroupSelectionState(owningGroup);
            }
        }

        /// <inheritdoc/>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (!IsEditing) return;

            var hit = HitTestRow(e.OriginalSource as DependencyObject);
            CommitEditIfClickedAway(hit.Row, hit.Column, null);
        }

        /// <summary>
        /// Walks up the visual tree from <paramref name="source"/> to find the
        /// containing <see cref="GridRowPresenter"/> (which provides the row via
        /// its <c>DataContext</c>) and the index of the
        /// <see cref="GridCellsPanel"/> child under the pointer (which provides
        /// the column via <see cref="GridControl.Columns"/>).
        /// </summary>
        /// <param name="source">The original source of the mouse event.</param>
        /// <returns>The resolved row and column, or both <c>null</c> if neither was found.</returns>
        private HitTestResult HitTestRow(DependencyObject source)
        {
            IGridRow row = null;
            GridColumn column = null;

            var current = source;
            while (current != null)
            {
                var presenter = current as GridRowPresenter;
                if (presenter != null)
                {
                    row = presenter.Row;
                    break;
                }

                var cellsPanel = VisualTreeHelper.GetParent(current) as GridCellsPanel;
                if (cellsPanel != null && column == null)
                {
                    int index = cellsPanel.Children.IndexOf(current as UIElement);
                    if (index >= 0 && index < Columns.Count)
                        column = Columns[index];
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return new HitTestResult(row, column);
        }

        /// <summary>
        /// Finds the <see cref="GridGroupRow"/> that owns <paramref name="item"/>
        /// by scanning <see cref="GridControl.DataSource"/>.
        /// </summary>
        /// <param name="item">The item row to find the owning group of.</param>
        /// <returns>The owning group, or <c>null</c> if not found.</returns>
        private GridGroupRow FindOwningGroup(GridItemRow item)
        {
            var source = DataSource;
            if (source == null) return null;

            for (int g = 0; g < source.GroupCount; g++)
            {
                var group = source.GetGroup(g);
                foreach (var candidate in group.Items)
                {
                    if (ReferenceEquals(candidate, item))
                        return group;
                }
            }

            return null;
        }

        private struct HitTestResult
        {
            public HitTestResult(IGridRow row, GridColumn column)
            {
                Row = row;
                Column = column;
            }

            public IGridRow Row { get; }
            public GridColumn Column { get; }
        }
    }
}
