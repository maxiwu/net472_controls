using System.Threading;
using System.Windows;
using CustomDataGrid.Models;
using CustomDataGrid.Selectors;
using NUnit.Framework;

namespace CustomDataGrid.Tests.Selectors
{
    /// <summary>
    /// Task 7.1: <see cref="GridRowStyleSelector"/> picks the group vs item row
    /// style by <c>IGridRow.Kind</c>.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class GridRowStyleSelectorTests
    {
        private static GridRowStyleSelector CreateSelector(out Style groupStyle, out Style itemStyle)
        {
            groupStyle = new Style();
            itemStyle = new Style();
            return new GridRowStyleSelector { GroupRowStyle = groupStyle, ItemRowStyle = itemStyle };
        }

        [Test]
        public void SelectStyle_GroupRow_ReturnsGroupRowStyle()
        {
            Style group, item;
            var selector = CreateSelector(out group, out item);

            var result = selector.SelectStyle(new GridGroupRow(), null);

            Assert.That(result, Is.SameAs(group));
        }

        [Test]
        public void SelectStyle_ItemRow_ReturnsItemRowStyle()
        {
            Style group, item;
            var selector = CreateSelector(out group, out item);

            var result = selector.SelectStyle(new GridItemRow(), null);

            Assert.That(result, Is.SameAs(item));
        }

        [Test]
        public void SelectStyle_UnknownItem_ReturnsNull()
        {
            Style group, item;
            var selector = CreateSelector(out group, out item);

            var result = selector.SelectStyle(new object(), null);

            Assert.That(result, Is.Null);
        }
    }
}
