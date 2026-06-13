using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using CustomDataGrid.Controls;

namespace CustomDataGrid.Columns
{
    /// <summary>
    /// A column that displays a bound value as text and edits it with a
    /// <see cref="ComboBox"/> populated from a column-level, shared
    /// <see cref="ItemsSource"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default <see cref="GridColumn.CellTemplate"/> is a <see cref="TextBlock"/>
    /// showing the display string (read mode). The
    /// <see cref="GridColumn.CellEditingTemplate"/> — a <see cref="ComboBox"/> —
    /// is only materialized for the cell currently being edited; it is never
    /// instantiated for non-editing rows.
    /// </para>
    /// <para>
    /// <see cref="ItemsSource"/> is set once at the column level and shared
    /// across every row's combo box. A per-cell <c>ItemsSource</c> would
    /// allocate a new enumerable per row, which is prohibited at scale.
    /// </para>
    /// <para>
    /// On selection commit, the cell raises <see cref="GridControl.CellEditCommitted"/>
    /// with the <b>display string</b> old and new values.
    /// </para>
    /// </remarks>
    public class ComboBoxColumn : GridColumn
    {
        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(ComboBoxColumn),
            new FrameworkPropertyMetadata(null, OnTemplateInputChanged));

        static ComboBoxColumn()
        {
            // Binding is declared on the base GridColumn, so override its
            // metadata here to add the template-rebuild callback. ItemsSource is
            // declared on this type, so its callback is set in Register above —
            // OverrideMetadata cannot target the declaring type.
            BindingProperty.OverrideMetadata(
                typeof(ComboBoxColumn),
                new FrameworkPropertyMetadata(null, OnTemplateInputChanged));
        }

        /// <summary>
        /// Gets or sets the shared list of selectable values, applied to every
        /// row's edit-mode <see cref="ComboBox.ItemsSource"/>.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnTemplateInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (ComboBoxColumn)d;
            column.RebuildTemplates();
        }

        private void RebuildTemplates()
        {
            var binding = Binding;
            if (binding == null) return;

            CellTemplate = BuildReadTemplate(binding);
            CellEditingTemplate = BuildEditTemplate(binding, ItemsSource);
        }

        private static DataTemplate BuildReadTemplate(BindingBase binding)
        {
            var factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetBinding(TextBlock.TextProperty, CloneBinding(binding, BindingMode.OneWay));
            factory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.SetValue(FrameworkElement.MarginProperty, new Thickness(4, 0, 4, 0));

            var template = new DataTemplate();
            template.VisualTree = factory;
            return template;
        }

        private static DataTemplate BuildEditTemplate(BindingBase binding, IEnumerable itemsSource)
        {
            var factory = new FrameworkElementFactory(typeof(ComboBox));
            factory.SetBinding(ComboBox.SelectedItemProperty, CloneBinding(binding, BindingMode.TwoWay));
            factory.SetValue(ComboBox.ItemsSourceProperty, itemsSource);
            factory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.AddHandler(FrameworkElement.LoadedEvent, (RoutedEventHandler)OnComboBoxLoaded);
            factory.AddHandler(Selector.SelectionChangedEvent, (SelectionChangedEventHandler)OnSelectionChanged);
            factory.AddHandler(UIElement.KeyDownEvent, (KeyEventHandler)OnComboBoxKeyDown);

            var template = new DataTemplate();
            template.VisualTree = factory;
            return template;
        }

        private static Binding CloneBinding(BindingBase bindingBase, BindingMode mode)
        {
            var source = bindingBase as Binding;
            if (source == null) return new Binding { Mode = mode };

            return new Binding(source.Path.Path)
            {
                Mode = mode,
                Converter = source.Converter,
                ConverterParameter = source.ConverterParameter,
                StringFormat = source.StringFormat
            };
        }

        private static void OnComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            comboBox.IsDropDownOpen = true;
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;

            var grid = EditingElementHelper.FindAncestorGridControl(comboBox);
            if (grid == null || !grid.IsEditing) return;

            grid.CommitEdit(comboBox.SelectedItem);
        }

        private static void OnComboBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;

            var comboBox = (ComboBox)sender;
            var grid = EditingElementHelper.FindAncestorGridControl(comboBox);
            if (grid != null) grid.CancelEdit();

            e.Handled = true;
        }
    }
}
