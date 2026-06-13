using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CustomDataGrid.Controls;
using NUnit.Framework;

namespace CustomDataGrid.Tests.Controls
{
    /// <summary>
    /// Task 5.10 sanity checks: verifies the virtualization properties set by
    /// the default <c>Generic.xaml</c> style are actually applied to a
    /// constructed <see cref="GridControl"/>. Each of these silently disables
    /// virtualization if wrong — see design doc §5.10.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class GridControlVirtualizationTests
    {
        private GridControl CreateLoadedGrid()
        {
            var grid = new GridControl { Width = 400, Height = 300 };
            var window = new Window
            {
                Content = grid,
                Width = 400,
                Height = 300,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false
            };
            window.Show();
            window.UpdateLayout();
            return grid;
        }

        [Test]
        public void DefaultStyle_SetsIsVirtualizingTrue()
        {
            var grid = CreateLoadedGrid();

            Assert.That(VirtualizingPanel.GetIsVirtualizing(grid), Is.True);
        }

        [Test]
        public void DefaultStyle_SetsVirtualizationModeRecycling()
        {
            var grid = CreateLoadedGrid();

            Assert.That(VirtualizingPanel.GetVirtualizationMode(grid), Is.EqualTo(VirtualizationMode.Recycling));
        }

        [Test]
        public void DefaultStyle_SetsCanContentScrollTrue()
        {
            var grid = CreateLoadedGrid();

            Assert.That(ScrollViewer.GetCanContentScroll(grid), Is.True);
        }

        [Test]
        public void DefaultStyle_SetsScrollUnitItem()
        {
            var grid = CreateLoadedGrid();

            Assert.That(VirtualizingPanel.GetScrollUnit(grid), Is.EqualTo(ScrollUnit.Item));
        }

        [Test]
        public void ItemsPanel_IsVirtualizingStackPanel()
        {
            var grid = CreateLoadedGrid();

            var itemsPanelTemplate = grid.ItemsPanel;
            Assert.That(itemsPanelTemplate, Is.Not.Null);

            var panel = itemsPanelTemplate.LoadContent();
            Assert.That(panel, Is.InstanceOf<VirtualizingStackPanel>());
        }
    }
}
