using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using CustomDataGrid.Columns;
using CustomDataGrid.Contracts;
using CustomDataGrid.Controls;
using CustomDataGrid.DataSources;
using CustomDataGrid.Models;
using NUnit.Framework;

namespace CustomDataGrid.Tests.Controls
{
    /// <summary>
    /// Rendering-level checks that the grid hosts editable columns and a
    /// two-way <c>SelectedRow</c> binding without the read-only
    /// <c>FlatRowCollection</c> / <c>ICollectionView</c> tripping a binding
    /// write-back error.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class GridControlEditBindingTests
    {
        private sealed class Vm : System.ComponentModel.INotifyPropertyChanged
        {
            private IGridRow _selectedRow;
            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
            public IGridRow SelectedRow
            {
                get { return _selectedRow; }
                set
                {
                    _selectedRow = value;
                    var h = PropertyChanged;
                    if (h != null) h(this, new System.ComponentModel.PropertyChangedEventArgs("SelectedRow"));
                }
            }
        }

        private static Window HostExpandedGrid(out GridControl grid, bool bindSelectedRow)
        {
            var item = new GridItemRow();
            var group = new GridGroupRow { Label = "G", IsExpanded = true, IsEnabled = true };
            group.Items.Add(item);
            group.TotalItemCount = 1;
            var source = new InMemoryGridDataSource(new List<GridGroupRow> { group });

            grid = new GridControl { Width = 400, Height = 300, DataSource = source };
            grid.Columns.Add(new TextColumn
            {
                Header = "Label",
                Width = new GridLength(200),
                Binding = new Binding("Label") { Mode = BindingMode.TwoWay }
            });
            grid.Rows.SetExpanded(0, true);

            if (bindSelectedRow)
                grid.SetBinding(GridControl.SelectedRowProperty, new Binding("SelectedRow") { Mode = BindingMode.TwoWay });

            return new Window
            {
                DataContext = new Vm(),
                Content = grid,
                Width = 400,
                Height = 300,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false
            };
        }

        [Test]
        public void ExpandedGroup_WithTwoWayTextColumn_DoesNotThrow()
        {
            GridControl grid;
            var window = HostExpandedGrid(out grid, bindSelectedRow: false);
            try
            {
                window.Show();
                window.UpdateLayout();
            }
            finally
            {
                window.Close();
            }
        }

        [Test]
        public void TwoWaySelectedRow_AgainstViewModel_DoesNotThrow()
        {
            GridControl grid;
            var window = HostExpandedGrid(out grid, bindSelectedRow: true);
            try
            {
                window.Show();
                window.UpdateLayout();
            }
            finally
            {
                window.Close();
            }
        }
    }
}
