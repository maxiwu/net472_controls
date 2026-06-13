using System.Threading;
using System.Windows;
using CustomDataGrid.Columns;
using CustomDataGrid.Controls;
using NUnit.Framework;

namespace CustomDataGrid.Tests.Controls
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ActionsMenuColumnTests
    {
        private sealed class PlainColumn : GridColumn
        {
        }

        [Test]
        public void Minimal()
        {
            var grid = new GridControl { Width = 400, Height = 300 };
            grid.Columns.Add(new PlainColumn { Width = new GridLength(60) });
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
            window.Close();

            Assert.That(grid, Is.Not.Null);
        }
    }
}
