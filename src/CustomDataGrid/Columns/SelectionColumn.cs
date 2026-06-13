using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CustomDataGrid.Contracts;
using CustomDataGrid.Converters;
using CustomDataGrid.Models;

namespace CustomDataGrid.Columns
{
    /// <summary>
    /// The built-in selection column. Inserted as the leftmost column when
    /// <see cref="Controls.GridControl.ShowSelectionColumn"/> is <c>true</c>, and
    /// removed when it is <c>false</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Item rows render a standard <see cref="CheckBox"/> reflecting
    /// <see cref="GridItemRow.IsSelected"/>; group rows render a tri-state
    /// <see cref="CheckBox"/> reflecting <see cref="GridGroupRow.SelectionState"/>
    /// (<c>IsChecked = null</c> for <see cref="SelectionState.PartiallySelected"/>),
    /// via <see cref="RowSelectionToCheckStateConverter"/>.
    /// </para>
    /// <para>
    /// The checkboxes are display-only (<see cref="UIElement.IsHitTestVisible"/> is
    /// <c>false</c>): clicking the cell falls through to the control's
    /// selection-column click path (see <c>GridControl.Mouse.cs</c>), which
    /// selects + highlights the row with Ctrl / Shift multi-select for item rows
    /// and applies the tri-state click rule for group rows. The column therefore
    /// does <b>not</b> set <see cref="GridColumn.SuppressRowSelectionOnClick"/>;
    /// it is, however, non-editable (<see cref="GridColumn.IsEditable"/> is
    /// <c>false</c>) so a click never enters cell edit mode.
    /// </para>
    /// <para>
    /// Disabled rows render their checkbox with <see cref="UIElement.IsEnabled"/>
    /// <c>false</c> (bound to <see cref="IGridRow.IsEnabled"/>); the click path
    /// already ignores disabled rows.
    /// </para>
    /// </remarks>
    public class SelectionColumn : GridColumn
    {
        static SelectionColumn()
        {
            IsEditableProperty.OverrideMetadata(typeof(SelectionColumn), new PropertyMetadata(false));
            WidthProperty.OverrideMetadata(
                typeof(SelectionColumn),
                new PropertyMetadata(new GridLength(36)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionColumn"/> class
        /// and builds its read template.
        /// </summary>
        public SelectionColumn()
        {
            CellTemplate = BuildTemplate();
        }

        private static DataTemplate BuildTemplate()
        {
            var root = new FrameworkElementFactory(typeof(Grid));

            root.AppendChild(BuildCheckBox(
                new Binding("IsSelected") { Mode = BindingMode.OneWay },
                RowKind.Item));

            root.AppendChild(BuildCheckBox(
                new Binding("SelectionState")
                {
                    Mode = BindingMode.OneWay,
                    Converter = RowSelectionToCheckStateConverter.Instance
                },
                RowKind.Group));

            var template = new DataTemplate();
            template.VisualTree = root;
            return template;
        }

        private static FrameworkElementFactory BuildCheckBox(BindingBase isCheckedBinding, RowKind visibleFor)
        {
            var factory = new FrameworkElementFactory(typeof(CheckBox));
            factory.SetValue(CheckBox.IsThreeStateProperty, visibleFor == RowKind.Group);
            factory.SetBinding(CheckBox.IsCheckedProperty, isCheckedBinding);
            factory.SetBinding(UIElement.IsEnabledProperty, new Binding("IsEnabled") { Mode = BindingMode.OneWay });
            factory.SetValue(UIElement.IsHitTestVisibleProperty, false);
            factory.SetValue(UIElement.FocusableProperty, false);
            factory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            factory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            // Show only for the matching row kind. Bound to IGridRow.Kind on the
            // row's DataContext; the other checkbox stays collapsed.
            var visibilityBinding = new Binding("Kind")
            {
                Mode = BindingMode.OneWay,
                Converter = RowKindToVisibilityConverter.Instance,
                ConverterParameter = visibleFor
            };
            factory.SetBinding(UIElement.VisibilityProperty, visibilityBinding);

            return factory;
        }
    }
}
