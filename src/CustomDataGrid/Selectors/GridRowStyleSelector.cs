using System.Windows;
using System.Windows.Controls;
using CustomDataGrid.Models;

namespace CustomDataGrid.Selectors
{
    /// <summary>
    /// Chooses the row container <see cref="Style"/> for a realized
    /// <c>GridRowPresenter</c> based on the kind of row it represents:
    /// <see cref="GroupRowStyle"/> for a <see cref="GridGroupRow"/> and
    /// <see cref="ItemRowStyle"/> for a <see cref="GridItemRow"/>.
    /// </summary>
    /// <remarks>
    /// Assigned to <c>GridControl.ItemContainerStyleSelector</c> (see
    /// <c>Themes/Generic.xaml</c>). The two styles are exposed as settable
    /// properties so a theme or a consumer can override either independently
    /// without replacing the selector.
    /// </remarks>
    public class GridRowStyleSelector : StyleSelector
    {
        /// <summary>
        /// Gets or sets the style applied to group header rows
        /// (<see cref="GridGroupRow"/>).
        /// </summary>
        public Style GroupRowStyle { get; set; }

        /// <summary>
        /// Gets or sets the style applied to data item rows
        /// (<see cref="GridItemRow"/>).
        /// </summary>
        public Style ItemRowStyle { get; set; }

        /// <inheritdoc/>
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is GridGroupRow)
                return GroupRowStyle;

            if (item is GridItemRow)
                return ItemRowStyle;

            return base.SelectStyle(item, container);
        }
    }
}
