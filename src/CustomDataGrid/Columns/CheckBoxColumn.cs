using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using CustomDataGrid.Controls;

namespace CustomDataGrid.Columns
{
    /// <summary>
    /// A column that displays and edits a <see cref="bool"/> value with a
    /// <see cref="CheckBox"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unlike <see cref="TextColumn"/> and <see cref="ComboBoxColumn"/>, this
    /// column has a single template — the checkbox toggles its value in place,
    /// with no separate editing mode. Toggling raises
    /// <see cref="GridControl.CellEditCommitted"/> directly (via
    /// <see cref="GridControl.CommitCellEdit"/>) with <see cref="bool"/> old and
    /// new values; it does not enter or affect <see cref="GridControl.EditingRow"/>.
    /// </para>
    /// <para>
    /// Clicking the checkbox cell does not change row selection or highlight —
    /// see <c>GridControl.Mouse.cs</c>'s click classification.
    /// </para>
    /// </remarks>
    public class CheckBoxColumn : GridColumn
    {
        static CheckBoxColumn()
        {
            BindingProperty.OverrideMetadata(
                typeof(CheckBoxColumn),
                new FrameworkPropertyMetadata(null, OnBindingChanged));

            IsEditableProperty.OverrideMetadata(typeof(CheckBoxColumn), new PropertyMetadata(false));
        }

        /// <inheritdoc/>
        public override bool SuppressRowSelectionOnClick
        {
            get { return true; }
        }

        private static void OnBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (CheckBoxColumn)d;
            column.RebuildTemplate();
        }

        private void RebuildTemplate()
        {
            var binding = Binding as Binding;
            if (binding == null) return;

            var checkBoxBinding = new Binding(binding.Path.Path)
            {
                Mode = BindingMode.TwoWay,
                Converter = binding.Converter,
                ConverterParameter = binding.ConverterParameter
            };

            var factory = new FrameworkElementFactory(typeof(CheckBox));
            factory.SetBinding(CheckBox.IsCheckedProperty, checkBoxBinding);
            factory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            factory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.AddHandler(ToggleButton.CheckedEvent, (RoutedEventHandler)OnChecked);
            factory.AddHandler(ToggleButton.UncheckedEvent, (RoutedEventHandler)OnUnchecked);

            var template = new DataTemplate();
            template.VisualTree = factory;
            CellTemplate = template;
        }

        private static void OnChecked(object sender, RoutedEventArgs e)
        {
            RaiseToggleCommitted((CheckBox)sender, true);
        }

        private static void OnUnchecked(object sender, RoutedEventArgs e)
        {
            RaiseToggleCommitted((CheckBox)sender, false);
        }

        private static void RaiseToggleCommitted(CheckBox checkBox, bool newValue)
        {
            var grid = EditingElementHelper.FindAncestorGridControl(checkBox);
            if (grid == null) return;

            var row = checkBox.DataContext;
            var column = EditingElementHelper.ResolveColumn(grid, checkBox);
            if (column == null) return;

            grid.CommitCellEdit((Contracts.IGridRow)row, column, !newValue, newValue);
        }
    }
}
