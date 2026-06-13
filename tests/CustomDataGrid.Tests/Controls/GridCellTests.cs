using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CustomDataGrid.Columns;
using CustomDataGrid.Contracts;
using CustomDataGrid.Controls;
using CustomDataGrid.DataSources;
using CustomDataGrid.Models;
using NUnit.Framework;

namespace CustomDataGrid.Tests.Controls
{
    /// <summary>
    /// Task 7.2: the <see cref="GridCell"/> attached properties that carry a
    /// cell's visual state from <c>GridCellsPanel</c> to the cell style triggers.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class GridCellTests
    {
        [Test]
        public void IsRowDisabled_DefaultsFalse()
        {
            Assert.That(GridCell.GetIsRowDisabled(new ContentControl()), Is.False);
        }

        [Test]
        public void IsRowHighlighted_DefaultsFalse()
        {
            Assert.That(GridCell.GetIsRowHighlighted(new ContentControl()), Is.False);
        }

        [Test]
        public void IsEditing_DefaultsFalse()
        {
            Assert.That(GridCell.GetIsEditing(new ContentControl()), Is.False);
        }

        [Test]
        public void IsRowDisabled_RoundTrips()
        {
            var cell = new ContentControl();
            GridCell.SetIsRowDisabled(cell, true);
            Assert.That(GridCell.GetIsRowDisabled(cell), Is.True);
        }

        [Test]
        public void IsRowHighlighted_RoundTrips()
        {
            var cell = new ContentControl();
            GridCell.SetIsRowHighlighted(cell, true);
            Assert.That(GridCell.GetIsRowHighlighted(cell), Is.True);
        }

        [Test]
        public void IsEditing_RoundTrips()
        {
            var cell = new ContentControl();
            GridCell.SetIsEditing(cell, true);
            Assert.That(GridCell.GetIsEditing(cell), Is.True);
        }

        [Test]
        public void GridCellsPanel_StampsRowDisabled_AndReactsToRowChange()
        {
            var item = new GridItemRow { IsEnabled = true };
            var group = new GridGroupRow { Label = "G", IsExpanded = true, IsEnabled = true };
            group.Items.Add(item);
            group.TotalItemCount = 1;

            var source = new InMemoryGridDataSource(new List<GridGroupRow> { group });

            var grid = new GridControl { Width = 400, Height = 300, DataSource = source };
            grid.Columns.Add(new TextColumn { Header = "Name", Width = new GridLength(200), Binding = new System.Windows.Data.Binding("Label") });
            grid.Rows.SetExpanded(0, true);

            var window = new Window
            {
                Content = grid,
                Width = 400,
                Height = 300,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false
            };

            try
            {
                window.Show();
                window.UpdateLayout();

                var itemCell = FindCellForRow(grid, item);
                Assert.That(itemCell, Is.Not.Null, "Expected a realized cell for the item row.");
                Assert.That(GridCell.GetIsRowDisabled(itemCell), Is.False);

                // Disabling the row should re-stamp the cell via the panel's
                // row PropertyChanged subscription.
                item.IsEnabled = false;
                window.UpdateLayout();

                Assert.That(GridCell.GetIsRowDisabled(itemCell), Is.True);
            }
            finally
            {
                window.Close();
            }
        }

        private static ContentControl FindCellForRow(DependencyObject root, IGridRow row)
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);

                var panel = child as GridCellsPanel;
                if (panel != null && ReferenceEquals(panel.DataContext, row) && panel.Children.Count > 0)
                    return panel.Children[0] as ContentControl;

                var found = FindCellForRow(child, row);
                if (found != null) return found;
            }

            return null;
        }
    }
}
