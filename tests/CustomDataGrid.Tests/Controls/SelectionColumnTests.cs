using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using CustomDataGrid.Columns;
using CustomDataGrid.Contracts;
using CustomDataGrid.Controls;
using CustomDataGrid.Converters;
using CustomDataGrid.Models;
using NUnit.Framework;

namespace CustomDataGrid.Tests.Controls
{
    /// <summary>
    /// Task 6.7: the built-in <see cref="SelectionColumn"/> and its supporting
    /// converters, plus the <see cref="GridControl.ShowSelectionColumn"/> change
    /// handler that inserts / removes it.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class SelectionColumnTests
    {
        // -------------------------------------------------------------- //
        //  ShowSelectionColumn insert / remove                            //
        // -------------------------------------------------------------- //

        [Test]
        public void ShowSelectionColumn_True_InsertsSelectionColumnAtIndexZero()
        {
            var grid = new GridControl();
            grid.Columns.Add(new TextColumn { Header = "Name" });

            grid.ShowSelectionColumn = true;

            Assert.That(grid.Columns.Count, Is.EqualTo(2));
            Assert.That(grid.Columns[0], Is.InstanceOf<SelectionColumn>());
            Assert.That(grid.Columns[1], Is.InstanceOf<TextColumn>());
        }

        [Test]
        public void ShowSelectionColumn_False_RemovesSelectionColumn()
        {
            var grid = new GridControl();
            grid.Columns.Add(new TextColumn { Header = "Name" });
            grid.ShowSelectionColumn = true;

            grid.ShowSelectionColumn = false;

            Assert.That(grid.Columns.Count, Is.EqualTo(1));
            Assert.That(grid.Columns.Any(c => c is SelectionColumn), Is.False);
        }

        [Test]
        public void ShowSelectionColumn_True_WhenColumnAlreadyPresent_DoesNotInsertDuplicate()
        {
            var grid = new GridControl();
            // Consumer pre-populated a selection column themselves.
            grid.Columns.Add(new SelectionColumn());
            grid.Columns.Add(new TextColumn { Header = "Name" });

            grid.ShowSelectionColumn = true;

            Assert.That(grid.Columns.Count(c => c is SelectionColumn), Is.EqualTo(1));
        }

        // -------------------------------------------------------------- //
        //  Column defaults                                                //
        // -------------------------------------------------------------- //

        [Test]
        public void SelectionColumn_IsNotEditable()
        {
            var column = new SelectionColumn();

            Assert.That(column.IsEditable, Is.False);
        }

        [Test]
        public void SelectionColumn_DoesNotSuppressRowSelectionOnClick()
        {
            // The cell click must fall through to the selection-column path
            // (select + highlight); suppressing would skip selection entirely.
            var column = new SelectionColumn();

            Assert.That(column.SuppressRowSelectionOnClick, Is.False);
        }

        [Test]
        public void SelectionColumn_HasCellTemplate()
        {
            var column = new SelectionColumn();

            Assert.That(column.CellTemplate, Is.Not.Null);
        }

        // -------------------------------------------------------------- //
        //  RowSelectionToCheckStateConverter                              //
        // -------------------------------------------------------------- //

        [Test]
        public void Converter_ItemSelected_ReturnsTrue()
        {
            var result = RowSelectionToCheckStateConverter.Instance.Convert(
                new GridItemRow { IsSelected = true }, typeof(bool?), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void Converter_ItemDeselected_ReturnsFalse()
        {
            var result = RowSelectionToCheckStateConverter.Instance.Convert(
                new GridItemRow { IsSelected = false }, typeof(bool?), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void Converter_GroupFullySelected_ReturnsTrue()
        {
            var result = RowSelectionToCheckStateConverter.Instance.Convert(
                new GridGroupRow { SelectionState = SelectionState.FullySelected },
                typeof(bool?), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void Converter_GroupPartiallySelected_ReturnsNull()
        {
            var result = RowSelectionToCheckStateConverter.Instance.Convert(
                new GridGroupRow { SelectionState = SelectionState.PartiallySelected },
                typeof(bool?), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Converter_GroupDeselected_ReturnsFalse()
        {
            var result = RowSelectionToCheckStateConverter.Instance.Convert(
                new GridGroupRow { SelectionState = SelectionState.Deselected },
                typeof(bool?), null, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        // -------------------------------------------------------------- //
        //  RowKindToVisibilityConverter                                   //
        // -------------------------------------------------------------- //

        [Test]
        public void KindConverter_MatchingKind_ReturnsVisible()
        {
            var result = RowKindToVisibilityConverter.Instance.Convert(
                RowKind.Item, typeof(Visibility), RowKind.Item, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void KindConverter_NonMatchingKind_ReturnsCollapsed()
        {
            var result = RowKindToVisibilityConverter.Instance.Convert(
                RowKind.Group, typeof(Visibility), RowKind.Item, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(Visibility.Collapsed));
        }
    }
}
