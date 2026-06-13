using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using CustomDataGrid.Columns;
using CustomDataGrid.Contracts;
using CustomDataGrid.Contracts.Events;
using CustomDataGrid.Controls;
using CustomDataGrid.Models;
using Moq;
using NUnit.Framework;

namespace CustomDataGrid.Tests.Controls
{
    /// <summary>
    /// Task 5.11 verification: confirms that hosting a <see cref="GridControl"/>
    /// over a 1,000,000-row <see cref="IGridDataSource"/> does not enumerate the
    /// entire source. Realized-container counts must stay independent of the row
    /// count; if the default <c>CollectionView</c> trap (sort / filter / group /
    /// Refresh — see design doc §5.9) were triggered, <see cref="IGridDataSource.GetGroup"/>
    /// would be called once per row instead (i.e. 1,000,000 times).
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class GridControlDataVirtualizationTests
    {
        private const int RowCount = 1_000_000;

        [Test]
        public void HostingOneMillionRows_OnlyFetchesRealizedContainers()
        {
            int getGroupCalls = 0;

            var source = new Mock<IGridDataSource>();
            source.Setup(s => s.GroupCount).Returns(RowCount);
            source.Setup(s => s.GetItemCount(It.IsAny<int>())).Returns(0);
            source.Setup(s => s.GetGroup(It.IsAny<int>()))
                .Returns((int i) =>
                {
                    Interlocked.Increment(ref getGroupCalls);
                    return new GridGroupRow { Label = "Group " + i, IsEnabled = true };
                });
            source.Setup(s => s.GetItems(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<GridItemRow>());

            var grid = new GridControl
            {
                Width = 400,
                Height = 300,
                DataSource = source.Object
            };
            grid.Columns.Add(new TestColumn { Header = "Name", Width = new GridLength(200) });

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

                Assert.That(grid.Rows.Count, Is.EqualTo(RowCount));

                // Realized containers (plus layout-pass overhead) must track the viewport
                // size, not the row count. An O(n) regression would call GetGroup in the
                // thousands or millions; a healthy run stays under a couple hundred.
                Assert.That(getGroupCalls, Is.LessThan(200),
                    "GetGroup was called " + getGroupCalls + " times — the CollectionView " +
                    "appears to be enumerating the entire data source instead of only " +
                    "realized containers.");
            }
            finally
            {
                window.Close();
            }
        }

        private sealed class TestColumn : GridColumn
        {
        }
    }
}
