using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CustomDataGrid.Controls;

namespace CustomDataGrid.Columns
{
    /// <summary>
    /// A column that displays a bound value as text and edits it with a
    /// <see cref="TextBox"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default <see cref="GridColumn.CellTemplate"/> is a <see cref="TextBlock"/>
    /// bound to <see cref="GridColumn.Binding"/>. The default
    /// <see cref="GridColumn.CellEditingTemplate"/> is a <see cref="TextBox"/>
    /// two-way bound to the same path.
    /// </para>
    /// <para>
    /// On commit (Enter, focus loss, or click-away — see
    /// <see cref="GridControl.CommitEditIfClickedAway"/>), the cell raises
    /// <see cref="GridControl.CellEditCommitted"/> with <see cref="string"/> old
    /// and new values. Escape raises <see cref="GridControl.CellEditCancelled"/>.
    /// </para>
    /// </remarks>
    public class TextColumn : GridColumn
    {
        static TextColumn()
        {
            BindingProperty.OverrideMetadata(
                typeof(TextColumn),
                new FrameworkPropertyMetadata(null, OnBindingChanged));
        }

        private static void OnBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (TextColumn)d;
            column.RebuildTemplates();
        }

        private void RebuildTemplates()
        {
            var binding = Binding;
            if (binding == null) return;

            CellTemplate = BuildReadTemplate(binding);
            CellEditingTemplate = BuildEditTemplate(binding);
        }

        private static DataTemplate BuildReadTemplate(BindingBase binding)
        {
            var factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetBinding(TextBlock.TextProperty, CloneBinding(binding));
            factory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.SetValue(FrameworkElement.MarginProperty, new Thickness(4, 0, 4, 0));

            var template = new DataTemplate();
            template.VisualTree = factory;
            return template;
        }

        private static DataTemplate BuildEditTemplate(BindingBase binding)
        {
            var editBinding = CloneBinding(binding);
            editBinding.Mode = BindingMode.TwoWay;
            editBinding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;

            var factory = new FrameworkElementFactory(typeof(TextBox));
            factory.SetBinding(TextBox.TextProperty, editBinding);
            factory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.AddHandler(FrameworkElement.LoadedEvent, (RoutedEventHandler)OnTextBoxLoaded);
            factory.AddHandler(UIElement.LostFocusEvent, (RoutedEventHandler)OnTextBoxLostFocus);
            factory.AddHandler(UIElement.KeyDownEvent, (KeyEventHandler)OnTextBoxKeyDown);

            var template = new DataTemplate();
            template.VisualTree = factory;
            return template;
        }

        private static Binding CloneBinding(BindingBase bindingBase)
        {
            var source = bindingBase as Binding;
            if (source == null) return new Binding();

            return new Binding(source.Path.Path)
            {
                Mode = source.Mode,
                Converter = source.Converter,
                ConverterParameter = source.ConverterParameter,
                StringFormat = source.StringFormat,
                TargetNullValue = source.TargetNullValue
            };
        }

        private static void OnTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            textBox.Focus();
            textBox.SelectAll();
        }

        private static void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            CommitTextBox((TextBox)sender);
        }

        private static void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (e.Key == Key.Enter)
            {
                CommitTextBox(textBox);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                var grid = EditingElementHelper.FindAncestorGridControl(textBox);
                if (grid != null) grid.CancelEdit();
                e.Handled = true;
            }
        }

        private static void CommitTextBox(TextBox textBox)
        {
            var grid = EditingElementHelper.FindAncestorGridControl(textBox);
            if (grid == null || !grid.IsEditing) return;

            var bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression != null)
                bindingExpression.UpdateSource();

            grid.CommitEdit(textBox.Text);
        }
    }
}
