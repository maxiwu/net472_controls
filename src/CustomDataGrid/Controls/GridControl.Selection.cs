using System.Collections.Generic;
using System.Linq;
using CustomDataGrid.Contracts;
using CustomDataGrid.Models;

namespace CustomDataGrid.Controls
{
    /// <summary>
    /// Selection model and tri-state group selection (Task 5.5). State lives on
    /// the rows themselves (<see cref="GridItemRow.IsSelected"/>,
    /// <see cref="GridGroupRow.SelectionState"/>) and is mirrored on the control
    /// via <see cref="GridControl.SelectedRow"/> / <see cref="GridControl.SelectedRows"/>.
    /// </summary>
    /// <remarks>
    /// Group selection cascade operates over <see cref="GridGroupRow.Items"/> —
    /// the items currently loaded for that group. Items not yet loaded by the
    /// data source are not part of the cascade.
    /// </remarks>
    public partial class GridControl
    {
        /// <summary>
        /// Replaces the current selection with a single row, clearing any
        /// previous selection. Updates <see cref="GridControl.SelectedRow"/> and
        /// <see cref="GridControl.SelectedRows"/>, sets
        /// <see cref="IGridRow.IsHighlighted"/>, and raises
        /// <see cref="GridControl.SelectedRowChanged"/> /
        /// <see cref="GridControl.SelectedRowsChanged"/>.
        /// </summary>
        /// <param name="row">The row to select.</param>
        public void SetSingleSelection(IGridRow row)
        {
            var oldRow = SelectedRow;
            var removed = new List<IGridRow>(SelectedRows);

            foreach (var r in removed)
                SetRowSelected(r, false);

            SelectedRows.Clear();

            var added = new List<IGridRow>();
            if (row != null)
            {
                SetRowSelected(row, true);
                SelectedRows.Add(row);
                added.Add(row);
            }

            SelectedRow = row;

            if (!ReferenceEquals(oldRow, row))
                RaiseSelectedRowChanged(oldRow, row);

            if (added.Count > 0 || removed.Count > 0)
                RaiseSelectedRowsChanged(added, removed);
        }

        /// <summary>
        /// Toggles whether <paramref name="row"/> is part of the multi-selection
        /// without affecting any other row's selection. Updates
        /// <see cref="GridControl.SelectedRows"/>,
        /// <see cref="IGridRow.IsHighlighted"/>, and raises
        /// <see cref="GridControl.SelectedRowsChanged"/>. No-ops when the row's
        /// <see cref="IGridRow.IsEnabled"/> is <c>false</c>.
        /// </summary>
        /// <param name="row">The row to toggle.</param>
        public void ToggleSelection(IGridRow row)
        {
            if (row == null || !row.IsEnabled) return;

            bool currentlySelected = IsRowSelected(row);
            SetRowSelected(row, !currentlySelected);

            if (currentlySelected)
            {
                SelectedRows.Remove(row);
                RaiseSelectedRowsChanged(new List<IGridRow>(), new List<IGridRow> { row });
            }
            else
            {
                SelectedRows.Add(row);
                RaiseSelectedRowsChanged(new List<IGridRow> { row }, new List<IGridRow>());
            }

            if (SelectedRows.Count == 1)
                SelectedRow = SelectedRows[0];
            else if (SelectedRow != null && !IsRowSelected(SelectedRow))
                SelectedRow = null;
        }

        /// <summary>
        /// Applies the group tri-state click rule: a group whose
        /// <see cref="GridGroupRow.SelectionState"/> is
        /// <see cref="SelectionState.Deselected"/> or
        /// <see cref="SelectionState.PartiallySelected"/> becomes
        /// <see cref="SelectionState.FullySelected"/> (selecting all enabled
        /// children); a group that is <see cref="SelectionState.FullySelected"/>
        /// becomes <see cref="SelectionState.Deselected"/> (deselecting all
        /// children). No-ops when the group's <see cref="IGridRow.IsEnabled"/> is
        /// <c>false</c>.
        /// </summary>
        /// <param name="group">The group row that was clicked.</param>
        public void SetGroupSelection(GridGroupRow group)
        {
            if (group == null || !group.IsEnabled) return;

            bool selectAll = group.SelectionState != SelectionState.FullySelected;

            var added = new List<IGridRow>();
            var removed = new List<IGridRow>();

            foreach (var item in group.Items)
            {
                if (!item.IsEnabled) continue;

                if (selectAll && !item.IsSelected)
                {
                    item.IsSelected = true;
                    item.IsHighlighted = true;
                    SelectedRows.Add(item);
                    added.Add(item);
                }
                else if (!selectAll && item.IsSelected)
                {
                    item.IsSelected = false;
                    item.IsHighlighted = false;
                    SelectedRows.Remove(item);
                    removed.Add(item);
                }
            }

            group.SelectionState = selectAll ? SelectionState.FullySelected : SelectionState.Deselected;

            if (added.Count > 0 || removed.Count > 0)
                RaiseSelectedRowsChanged(added, removed);
        }

        /// <summary>
        /// Recomputes <paramref name="group"/>'s
        /// <see cref="GridGroupRow.SelectionState"/> from the selection state of
        /// its enabled children: <see cref="SelectionState.FullySelected"/> when
        /// all are selected, <see cref="SelectionState.Deselected"/> when none
        /// are, and <see cref="SelectionState.PartiallySelected"/> otherwise. A
        /// group with no enabled children is <see cref="SelectionState.Deselected"/>.
        /// </summary>
        /// <param name="group">The group whose selection state to recompute.</param>
        public void RecomputeGroupSelectionState(GridGroupRow group)
        {
            if (group == null) return;

            var enabledChildren = group.Items.Where(i => i.IsEnabled).ToList();

            if (enabledChildren.Count == 0)
            {
                group.SelectionState = SelectionState.Deselected;
                return;
            }

            int selectedCount = enabledChildren.Count(i => i.IsSelected);

            if (selectedCount == 0)
                group.SelectionState = SelectionState.Deselected;
            else if (selectedCount == enabledChildren.Count)
                group.SelectionState = SelectionState.FullySelected;
            else
                group.SelectionState = SelectionState.PartiallySelected;
        }

        private static bool IsRowSelected(IGridRow row)
        {
            var item = row as GridItemRow;
            if (item != null) return item.IsSelected;

            var group = row as GridGroupRow;
            if (group != null) return group.SelectionState == SelectionState.FullySelected;

            return false;
        }

        private static void SetRowSelected(IGridRow row, bool selected)
        {
            var item = row as GridItemRow;
            if (item != null)
            {
                item.IsSelected = selected;
                item.IsHighlighted = selected;
                return;
            }

            var group = row as GridGroupRow;
            if (group != null)
            {
                group.SelectionState = selected ? SelectionState.FullySelected : SelectionState.Deselected;
                group.IsHighlighted = selected;
            }
        }
    }
}
