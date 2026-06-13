using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CustomDataGrid.Columns;

namespace CustomDataGrid.Controls
{
    /// <summary>
    /// A custom <see cref="Panel"/>, modeled on <c>DataGridCellsPanel</c>, that
    /// arranges one cell per column using the widths published by the shared
    /// <see cref="GridColumnCollection"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="MeasureOverride"/> does not measure children to determine
    /// column widths. Each child is measured against its corresponding
    /// <see cref="GridColumn.Width"/> (resolving star widths against the
    /// available width), then arranged horizontally by accumulated offsets. This
    /// keeps column sizing virtualization-safe — see design doc §5.4.
    /// </para>
    /// <para>
    /// The panel subscribes to <see cref="GridColumnCollection.ColumnWidthChanged"/>
    /// and calls <see cref="UIElement.InvalidateMeasure"/> so width changes
    /// propagate to every realized row without re-realizing them.
    /// </para>
    /// </remarks>
    public class GridCellsPanel : Panel
    {
        /// <summary>
        /// Minimum height, in device-independent pixels, that a data row is
        /// measured to even when its cells render no content. A row that
        /// measured to zero height would defeat <c>VirtualizingStackPanel</c>:
        /// the panel can never fill its viewport with zero-height items, so it
        /// keeps realizing containers until it has materialized the entire data
        /// source (millions of rows), exhausting memory. Flooring the row height
        /// keeps virtualization correct regardless of cell content. Not applied
        /// to the header panel (see <see cref="IsHeaderPanel"/>).
        /// </summary>
        private const double MinRowHeight = 22d;

        private GridControl _grid;
        private bool _isLoaded;
        private Contracts.IGridRow _subscribedRow;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridCellsPanel"/> class.
        /// </summary>
        public GridCellsPanel()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;

            _grid = FindAncestorGridControl();
            if (_grid != null)
                _grid.EditStateChanged += OnEditStateChanged;

            RebuildCells();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = false;

            if (_grid != null)
            {
                _grid.EditStateChanged -= OnEditStateChanged;
                _grid = null;
            }

            UnsubscribeRow();
        }

        private void OnEditStateChanged(object sender, EventArgs e)
        {
            RefreshCellTemplates();
            RefreshCellVisualStates();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // The container is recycled onto a different row: re-target the
            // row PropertyChanged subscription and re-stamp cell visual state.
            UnsubscribeRow();
            SubscribeRow();
            RefreshCellVisualStates();
        }

        private void SubscribeRow()
        {
            if (IsHeaderPanel) return;

            var row = DataContext as Contracts.IGridRow;
            _subscribedRow = row;

            var inpc = row as System.ComponentModel.INotifyPropertyChanged;
            if (inpc != null)
                inpc.PropertyChanged += OnRowPropertyChanged;
        }

        private void UnsubscribeRow()
        {
            var inpc = _subscribedRow as System.ComponentModel.INotifyPropertyChanged;
            if (inpc != null)
                inpc.PropertyChanged -= OnRowPropertyChanged;

            _subscribedRow = null;
        }

        private void OnRowPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsEnabled" || e.PropertyName == "IsHighlighted" || e.PropertyName == null)
                RefreshCellVisualStates();
        }

        private GridControl FindAncestorGridControl()
        {
            DependencyObject current = this;
            while (current != null)
            {
                var grid = current as GridControl;
                if (grid != null) return grid;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        /// <summary>
        /// Identifies the <see cref="IsHeaderPanel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsHeaderPanelProperty = DependencyProperty.Register(
            nameof(IsHeaderPanel),
            typeof(bool),
            typeof(GridCellsPanel),
            new PropertyMetadata(false, OnIsHeaderPanelChanged));

        private static void OnIsHeaderPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (GridCellsPanel)d;
            if (panel._isLoaded)
                panel.RebuildCells();
        }

        /// <summary>
        /// Gets or sets whether this panel is the column-headers row (hosted by
        /// <see cref="GridColumnHeadersPresenter"/>) rather than a data row
        /// (hosted by <see cref="GridRowPresenter"/>). When <c>true</c>, each
        /// cell shows the corresponding column's <see cref="GridColumn.Header"/>
        /// instead of binding to row data, and <see cref="GridColumn.CellTemplate"/>
        /// is not applied.
        /// </summary>
        public bool IsHeaderPanel
        {
            get { return (bool)GetValue(IsHeaderPanelProperty); }
            set { SetValue(IsHeaderPanelProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Columns"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            nameof(Columns),
            typeof(GridColumnCollection),
            typeof(GridCellsPanel),
            new PropertyMetadata(null, OnColumnsChanged));

        /// <summary>
        /// Gets or sets the shared column collection this panel lays cells out
        /// against. Typically bound to <c>GridControl.Columns</c>.
        /// </summary>
        public GridColumnCollection Columns
        {
            get { return (GridColumnCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (GridCellsPanel)d;

            var oldColumns = e.OldValue as GridColumnCollection;
            if (oldColumns != null)
            {
                oldColumns.ColumnWidthChanged -= panel.OnColumnWidthChanged;
                oldColumns.CollectionChanged -= panel.OnColumnsCollectionChanged;
            }

            var newColumns = e.NewValue as GridColumnCollection;
            if (newColumns != null)
            {
                newColumns.ColumnWidthChanged += panel.OnColumnWidthChanged;
                newColumns.CollectionChanged += panel.OnColumnsCollectionChanged;
            }

            if (panel._isLoaded)
                panel.RebuildCells();
        }

        private void OnColumnWidthChanged(object sender, EventArgs e)
        {
            InvalidateMeasure();
        }

        private void OnColumnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isLoaded)
                RebuildCells();
        }

        /// <summary>
        /// Regenerates one cell per column. Each cell is a <see cref="ContentControl"/>
        /// whose <see cref="ContentControl.Content"/> is the row itself (bound via
        /// <c>"."</c>, so the cell template's bindings resolve against the row's
        /// properties) and whose <see cref="ContentControl.ContentTemplate"/> is
        /// the column's <see cref="GridColumn.CellTemplate"/>, swapped for
        /// <see cref="GridColumn.CellEditingTemplate"/> while that cell is being
        /// edited (see <see cref="RefreshCellTemplates"/>). For the header panel
        /// (<see cref="IsHeaderPanel"/>), each cell shows the column's
        /// <see cref="GridColumn.Header"/> instead.
        /// </summary>
        private void RebuildCells()
        {
            Children.Clear();

            var columns = Columns;
            if (columns == null) return;

            var cellStyle = IsHeaderPanel ? null : TryFindResource("GridCellStyle") as Style;

            foreach (var column in columns)
            {
                var cell = new ContentControl();

                if (IsHeaderPanel)
                {
                    cell.Content = column.Header;
                }
                else
                {
                    cell.SetBinding(ContentControl.ContentProperty, new Binding("."));
                    if (cellStyle != null)
                        cell.Style = cellStyle;
                }

                Children.Add(cell);
            }

            if (!IsHeaderPanel && _subscribedRow == null)
                SubscribeRow();

            RefreshCellTemplates();
            RefreshCellVisualStates();
            InvalidateMeasure();
        }

        /// <summary>
        /// Stamps each cell's visual-state attached properties
        /// (<see cref="GridCell.IsRowDisabledProperty"/>,
        /// <see cref="GridCell.IsRowHighlightedProperty"/>,
        /// <see cref="GridCell.IsEditingProperty"/>) from the current row and grid
        /// edit state, so the cell <c>Style</c> triggers in <c>Generic.xaml</c> can
        /// react. No-op for the header panel (<see cref="IsHeaderPanel"/>).
        /// </summary>
        /// <remarks>
        /// The disabled-group cascade needs no handling here: a disabled group
        /// cannot expand (see <c>FlatRowCollection.ExpandGroup</c>), so its item
        /// rows are never realized while it is disabled. A cell's row-disabled
        /// state is therefore just the row's own <see cref="Contracts.IGridRow.IsEnabled"/>.
        /// </remarks>
        private void RefreshCellVisualStates()
        {
            if (IsHeaderPanel) return;

            var columns = Columns;
            if (columns == null) return;

            var row = DataContext as Contracts.IGridRow;
            var grid = _grid;

            bool rowDisabled = row != null && !row.IsEnabled;
            bool rowHighlighted = row != null && row.IsHighlighted;

            for (int i = 0; i < Children.Count && i < columns.Count; i++)
            {
                var cell = (ContentControl)Children[i];
                var column = columns[i];

                bool isEditingThisCell = grid != null && grid.IsEditing
                    && ReferenceEquals(grid.EditingRow, row)
                    && ReferenceEquals(grid.EditingColumn, column);

                GridCell.SetIsRowDisabled(cell, rowDisabled);
                GridCell.SetIsRowHighlighted(cell, rowHighlighted);
                GridCell.SetIsEditing(cell, isEditingThisCell);
            }
        }

        /// <summary>
        /// Re-evaluates which cell template each realized cell should use, based
        /// on the current edit state of the ancestor <see cref="GridControl"/>.
        /// Called after a rebuild and whenever <see cref="GridControl.EditStateChanged"/>
        /// fires. No-op for the header panel (<see cref="IsHeaderPanel"/>).
        /// </summary>
        private void RefreshCellTemplates()
        {
            if (IsHeaderPanel) return;

            var columns = Columns;
            if (columns == null) return;

            var row = DataContext as Contracts.IGridRow;
            var grid = _grid;

            for (int i = 0; i < Children.Count && i < columns.Count; i++)
            {
                var cell = (ContentControl)Children[i];
                var column = columns[i];

                bool isEditingThisCell = grid != null && grid.IsEditing
                    && ReferenceEquals(grid.EditingRow, row)
                    && ReferenceEquals(grid.EditingColumn, column);

                var template = isEditingThisCell && column.CellEditingTemplate != null
                    ? column.CellEditingTemplate
                    : column.CellTemplate;

                if (!ReferenceEquals(cell.ContentTemplate, template))
                    cell.ContentTemplate = template;
            }
        }

        /// <summary>
        /// Resolves each column's pixel width against <paramref name="availableWidth"/>
        /// and the panel's children, then measures and arranges accordingly.
        /// </summary>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (Children.Count == 0 && Columns != null)
                RebuildCells();

            var widths = ResolveColumnWidths(availableSize.Width);
            double height = 0;

            for (int i = 0; i < Children.Count && i < widths.Length; i++)
            {
                var child = Children[i];
                child.Measure(new Size(widths[i], availableSize.Height));
                height = Math.Max(height, child.DesiredSize.Height);
            }

            double total = 0;
            foreach (var w in widths) total += w;

            // Floor data-row height so a contentless row never collapses to 0px
            // and silently disables virtualization (see MinRowHeight).
            if (!IsHeaderPanel && height < MinRowHeight)
                height = MinRowHeight;

            double resolvedWidth = double.IsInfinity(availableSize.Width) ? total : availableSize.Width;
            return new Size(resolvedWidth, height);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var widths = ResolveColumnWidths(finalSize.Width);

            double x = 0;
            for (int i = 0; i < Children.Count && i < widths.Length; i++)
            {
                var child = Children[i];
                child.Arrange(new Rect(x, 0, widths[i], finalSize.Height));
                x += widths[i];
            }

            return finalSize;
        }

        /// <summary>
        /// Resolves each column's width in device-independent pixels against
        /// <paramref name="availableWidth"/>: fixed widths are used as-is, star
        /// widths share the remaining space proportionally.
        /// </summary>
        private double[] ResolveColumnWidths(double availableWidth)
        {
            var columns = Columns;
            int count = columns != null ? columns.Count : 0;
            var widths = new double[count];

            if (count == 0) return widths;

            double fixedTotal = 0;
            double starTotal = 0;

            for (int i = 0; i < count; i++)
            {
                var width = columns[i].Width;
                if (width.IsAbsolute)
                    fixedTotal += Clamp(columns[i], width.Value);
                else
                    starTotal += width.Value;
            }

            double remaining = double.IsInfinity(availableWidth)
                ? 0
                : Math.Max(0, availableWidth - fixedTotal);

            for (int i = 0; i < count; i++)
            {
                var column = columns[i];
                var width = column.Width;

                if (width.IsAbsolute)
                {
                    widths[i] = Clamp(column, width.Value);
                }
                else if (starTotal > 0)
                {
                    double share = remaining * (width.Value / starTotal);
                    widths[i] = Clamp(column, share);
                }
                else
                {
                    widths[i] = 0;
                }
            }

            return widths;
        }

        private static double Clamp(GridColumn column, double value)
        {
            if (value < column.MinWidth) value = column.MinWidth;
            if (value > column.MaxWidth) value = column.MaxWidth;
            return value;
        }
    }
}
