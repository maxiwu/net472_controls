using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CustomDataGrid.Controls;

namespace CustomDataGrid.Columns
{
    /// <summary>
    /// Shared helper for editing-template elements (<see cref="TextColumn"/>,
    /// <see cref="ComboBoxColumn"/>) to locate the ancestor <see cref="GridControl"/>
    /// so they can call <see cref="GridControl.CommitEdit"/> /
    /// <see cref="GridControl.CancelEdit"/>.
    /// </summary>
    internal static class EditingElementHelper
    {
        /// <summary>
        /// Walks up the visual tree from <paramref name="element"/> to find the
        /// containing <see cref="GridControl"/>, or <c>null</c> if none is found.
        /// </summary>
        /// <param name="element">The element to start the search from.</param>
        public static GridControl FindAncestorGridControl(DependencyObject element)
        {
            var current = element;
            while (current != null)
            {
                var grid = current as GridControl;
                if (grid != null) return grid;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        /// <summary>
        /// Walks up the visual tree from <paramref name="element"/> to find the
        /// <see cref="ContentControl"/> cell hosted directly by the row's
        /// <c>GridCellsPanel</c>, and resolves it to the corresponding
        /// <see cref="GridColumn"/> by index.
        /// </summary>
        /// <param name="grid">The owning grid, whose <see cref="GridControl.Columns"/>
        /// supplies the column at the resolved index.</param>
        /// <param name="element">The element to start the search from.</param>
        public static GridColumn ResolveColumn(GridControl grid, DependencyObject element)
        {
            DependencyObject current = element;
            while (current != null)
            {
                var parent = VisualTreeHelper.GetParent(current);
                var panel = parent as GridCellsPanel;
                if (panel != null)
                {
                    int index = panel.Children.IndexOf(current as UIElement);
                    if (index >= 0 && index < grid.Columns.Count)
                        return grid.Columns[index];

                    return null;
                }

                current = parent;
            }

            return null;
        }
    }
}
