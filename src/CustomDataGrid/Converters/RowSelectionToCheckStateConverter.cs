using System;
using System.Globalization;
using System.Windows.Data;
using CustomDataGrid.Contracts;
using CustomDataGrid.Models;

namespace CustomDataGrid.Converters
{
    /// <summary>
    /// One-way converts an <see cref="IGridRow"/> to the nullable
    /// <see cref="bool"/> used by a tri-state <see cref="System.Windows.Controls.CheckBox.IsChecked"/>:
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>A <see cref="GridItemRow"/> maps to its <see cref="GridItemRow.IsSelected"/> (<c>true</c> / <c>false</c>).</item>
    /// <item>A <see cref="GridGroupRow"/> maps from its <see cref="GridGroupRow.SelectionState"/>:
    /// <see cref="SelectionState.FullySelected"/> → <c>true</c> (tick),
    /// <see cref="SelectionState.PartiallySelected"/> → <c>null</c> (solid square),
    /// <see cref="SelectionState.Deselected"/> → <c>false</c> (empty box).</item>
    /// </list>
    /// <para>
    /// Conversion is one-way: the built-in selection column's checkbox only
    /// reflects state. Clicking the cell routes through the control's
    /// selection-column click path (see <c>GridControl.Mouse.cs</c>), which owns
    /// the selection bookkeeping (<c>SelectedRows</c>, highlight, tri-state
    /// transitions). Binding the checkbox two-way would bypass that and is not
    /// supported.
    /// </para>
    /// </remarks>
    public sealed class RowSelectionToCheckStateConverter : IValueConverter
    {
        /// <summary>
        /// Gets the shared instance of this converter, for use as a static
        /// resource reference from XAML.
        /// </summary>
        public static readonly RowSelectionToCheckStateConverter Instance = new RowSelectionToCheckStateConverter();

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as GridItemRow;
            if (item != null)
                return item.IsSelected;

            var group = value as GridGroupRow;
            if (group != null)
            {
                switch (group.SelectionState)
                {
                    case SelectionState.FullySelected:
                        return true;
                    case SelectionState.PartiallySelected:
                        return null;
                    default:
                        return false;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
