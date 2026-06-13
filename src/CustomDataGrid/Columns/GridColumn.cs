using System.Windows;
using System.Windows.Data;

namespace CustomDataGrid.Columns
{
    /// <summary>
    /// Base class for a column definition in a <see cref="Controls.GridControl"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="GridColumn"/> is a plain <see cref="DependencyObject"/> — it is
    /// deliberately not a <c>System.Windows.Controls.DataGridColumn</c>. Column
    /// instances live in the shared <see cref="GridColumnCollection"/> exposed by
    /// <c>GridControl.Columns</c>, and both the header row
    /// (<c>GridColumnHeadersPresenter</c>) and every realized row's
    /// <c>GridCellsPanel</c> read the same <see cref="Width"/> value from the same
    /// instances. Widths flow down from this collection to the header and rows;
    /// rows never measure up to publish a width back. See design doc §5.4.
    /// </para>
    /// </remarks>
    public abstract class GridColumn : DependencyObject
    {
        /// <summary>
        /// Identifies the <see cref="Width"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            nameof(Width),
            typeof(GridLength),
            typeof(GridColumn),
            new PropertyMetadata(new GridLength(1, GridUnitType.Star)));

        /// <summary>
        /// Identifies the <see cref="MinWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register(
            nameof(MinWidth),
            typeof(double),
            typeof(GridColumn),
            new PropertyMetadata(0d));

        /// <summary>
        /// Identifies the <see cref="MaxWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.Register(
            nameof(MaxWidth),
            typeof(double),
            typeof(GridColumn),
            new PropertyMetadata(double.PositiveInfinity));

        /// <summary>
        /// Identifies the <see cref="Header"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(GridColumn),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="IsEditable"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register(
            nameof(IsEditable),
            typeof(bool),
            typeof(GridColumn),
            new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="ColumnName"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnNameProperty = DependencyProperty.Register(
            nameof(ColumnName),
            typeof(string),
            typeof(GridColumn),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Binding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BindingProperty = DependencyProperty.Register(
            nameof(Binding),
            typeof(BindingBase),
            typeof(GridColumn),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="CellTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CellTemplateProperty = DependencyProperty.Register(
            nameof(CellTemplate),
            typeof(DataTemplate),
            typeof(GridColumn),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="CellEditingTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CellEditingTemplateProperty = DependencyProperty.Register(
            nameof(CellEditingTemplate),
            typeof(DataTemplate),
            typeof(GridColumn),
            new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the column's width. May be a fixed pixel value
        /// (<c>180</c>) or a star value (<c>*</c>, <c>2*</c>). Star widths are
        /// resolved by <c>GridCellsPanel</c> against the available viewport
        /// width during arrange — never against rendered cell content.
        /// </summary>
        public GridLength Width
        {
            get { return (GridLength)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the minimum resolved width, in device-independent pixels.
        /// </summary>
        public double MinWidth
        {
            get { return (double)GetValue(MinWidthProperty); }
            set { SetValue(MinWidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the maximum resolved width, in device-independent pixels.
        /// </summary>
        public double MaxWidth
        {
            get { return (double)GetValue(MaxWidthProperty); }
            set { SetValue(MaxWidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the content displayed in the column's header cell.
        /// </summary>
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether cells in this column can enter edit mode.
        /// When <c>false</c>, clicking a cell in this column selects the row
        /// instead of editing it.
        /// </summary>
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        /// <summary>
        /// Gets or sets the logical name of the column. Surfaced as
        /// <see cref="Contracts.Events.CellEditCommittedEventArgs.ColumnName"/> /
        /// <see cref="Contracts.Events.CellEditCancelledEventArgs.ColumnName"/>
        /// when a cell in this column is edited.
        /// </summary>
        public string ColumnName
        {
            get { return (string)GetValue(ColumnNameProperty); }
            set { SetValue(ColumnNameProperty, value); }
        }

        /// <summary>
        /// Gets or sets the binding used to read (and, for editable columns,
        /// write) the cell's value from the row's <see cref="System.Windows.FrameworkElement.DataContext"/>
        /// (an <see cref="Contracts.IGridRow"/>). Applied to the content of the
        /// realized cell — for <see cref="CellTemplate"/> templates that bind to
        /// a single value (e.g. a <c>TextBlock.Text</c>), <see cref="Controls.GridCellsPanel"/>
        /// applies this binding to the cell's <c>ContentControl.Content</c>.
        /// </summary>
        public BindingBase Binding
        {
            get { return (BindingBase)GetValue(BindingProperty); }
            set { SetValue(BindingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the read-mode cell template. If not set, the column
        /// type's default template is used.
        /// </summary>
        public DataTemplate CellTemplate
        {
            get { return (DataTemplate)GetValue(CellTemplateProperty); }
            set { SetValue(CellTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the edit-mode cell template. When <c>null</c>, the cell
        /// remains in its <see cref="CellTemplate"/> while editing (used by
        /// columns such as <see cref="CheckBoxColumn"/> that have no separate
        /// editing visual).
        /// </summary>
        public DataTemplate CellEditingTemplate
        {
            get { return (DataTemplate)GetValue(CellEditingTemplateProperty); }
            set { SetValue(CellEditingTemplateProperty, value); }
        }

        /// <summary>
        /// Gets whether clicking a cell in this column should leave row
        /// selection and highlight unchanged. <c>true</c> for columns whose
        /// cells act on click (<see cref="CheckBoxColumn"/>,
        /// <see cref="ButtonColumn"/>, <see cref="ActionsMenuColumn"/>, and the
        /// built-in selection column, which applies its own selection logic).
        /// The base implementation returns <c>false</c>.
        /// </summary>
        public virtual bool SuppressRowSelectionOnClick
        {
            get { return false; }
        }

        /// <summary>
        /// Evaluates <see cref="Binding"/> against <paramref name="row"/> and
        /// returns the resulting value, or <c>null</c> if <see cref="Binding"/>
        /// is not set. Used to capture a cell's current value as the "old value"
        /// when a cell edit begins.
        /// </summary>
        /// <param name="row">The row to evaluate the binding against.</param>
        public object GetCellValue(object row)
        {
            var binding = Binding;
            if (binding == null || row == null) return null;

            var dummy = new DependencyObject();
            BindingOperations.SetBinding(dummy, ValueHolder.ValueProperty, CloneBindingWithSource(binding, row));
            return dummy.GetValue(ValueHolder.ValueProperty);
        }

        private static BindingBase CloneBindingWithSource(BindingBase bindingBase, object source)
        {
            var binding = bindingBase as Binding;
            if (binding == null) return bindingBase;

            return new Binding(binding.Path.Path)
            {
                Source = source,
                Mode = BindingMode.OneWay,
                Converter = binding.Converter,
                ConverterParameter = binding.ConverterParameter,
                StringFormat = binding.StringFormat
            };
        }

        /// <summary>
        /// A private attached-property holder used by <see cref="GetValue(object)"/>
        /// to evaluate a binding off-tree.
        /// </summary>
        private static class ValueHolder
        {
            public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
                "Value",
                typeof(object),
                typeof(ValueHolder),
                new PropertyMetadata(null));
        }
    }
}
