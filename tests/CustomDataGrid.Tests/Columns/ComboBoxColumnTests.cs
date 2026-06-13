using System.Threading;
using System.Windows;
using System.Windows.Data;
using CustomDataGrid.Columns;
using NUnit.Framework;

namespace CustomDataGrid.Tests.Columns
{
    /// <summary>
    /// Regression tests for <see cref="ComboBoxColumn"/>. Constructing the column
    /// used to throw a <see cref="System.TypeInitializationException"/> because its
    /// static constructor called <c>OverrideMetadata</c> on
    /// <see cref="ComboBoxColumn.ItemsSourceProperty"/>, which the column itself
    /// declares — <c>OverrideMetadata</c> may only target an inherited property.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ComboBoxColumnTests
    {
        [Test]
        public void Construct_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => { var _ = new ComboBoxColumn(); });
        }

        [Test]
        public void SettingBindingAndItemsSource_BuildsTemplates()
        {
            var column = new ComboBoxColumn
            {
                ItemsSource = new[] { "Enable", "Disable" },
                Binding = new Binding("Status") { Mode = BindingMode.TwoWay }
            };

            Assert.That(column.CellTemplate, Is.Not.Null);
            Assert.That(column.CellEditingTemplate, Is.Not.Null);
        }
    }
}
