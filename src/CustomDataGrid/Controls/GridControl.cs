using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using CustomDataGrid.Collection;
using CustomDataGrid.Columns;
using CustomDataGrid.Contracts;

namespace CustomDataGrid.Controls
{
    /// <summary>
    /// The grid control. Extends <see cref="ItemsControl"/> directly — not
    /// <see cref="DataGrid"/>, not <see cref="Selector"/> /
    /// <see cref="MultiSelector"/>. See design doc §5.1 for the rationale: the
    /// control's selection model, click-vs-edit resolution, tri-state group
    /// selection, disabled-row handling, and 2M-row virtualization with
    /// on-demand fetching from <see cref="IGridDataSource"/> all diverge from
    /// those base classes' built-in behavior at every interaction point.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Data virtualization warning.</b> Setting <see cref="ItemsControl.ItemsSource"/>
    /// wraps the source in a default <c>CollectionView</c>. Do not apply
    /// <c>SortDescriptions</c>, a <c>Filter</c>, or <c>GroupDescriptions</c> to
    /// that view, and do not call <c>CollectionView.Refresh()</c> — any of these
    /// can enumerate the entire <see cref="IGridDataSource"/>, silently
    /// materializing every row (up to millions) even though UI virtualization
    /// still appears to work. If sorting, filtering, or grouping is required, it
    /// must be implemented at the data-source level. See design doc §5.9.
    /// </para>
    /// </remarks>
    public partial class GridControl : ItemsControl
    {
        static GridControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GridControl),
                new FrameworkPropertyMetadata(typeof(GridControl)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridControl"/> class.
        /// </summary>
        public GridControl()
        {
            SelectedRows = new ObservableCollection<IGridRow>();
            Columns = new GridColumnCollection();
            InitializeCommands();
        }

        // -------------------------------------------------------------- //
        //  Dependency Properties (Task 5.2)                                //
        // -------------------------------------------------------------- //

        /// <summary>
        /// Identifies the <see cref="DataSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
            nameof(DataSource),
            typeof(IGridDataSource),
            typeof(GridControl),
            new PropertyMetadata(null, OnDataSourceChanged));

        /// <summary>
        /// Identifies the <see cref="Columns"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            nameof(Columns),
            typeof(GridColumnCollection),
            typeof(GridControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="SelectedRow"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedRowProperty = DependencyProperty.Register(
            nameof(SelectedRow),
            typeof(IGridRow),
            typeof(GridControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Identifies the <see cref="SelectedRows"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedRowsProperty = DependencyProperty.Register(
            nameof(SelectedRows),
            typeof(ObservableCollection<IGridRow>),
            typeof(GridControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Identifies the <see cref="HeaderActions"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderActionsProperty = DependencyProperty.Register(
            nameof(HeaderActions),
            typeof(System.Collections.Generic.IList<IGridHeaderAction>),
            typeof(GridControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="GroupRowActions"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GroupRowActionsProperty = DependencyProperty.Register(
            nameof(GroupRowActions),
            typeof(System.Collections.Generic.IList<IGridRowAction>),
            typeof(GridControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ItemRowActions"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemRowActionsProperty = DependencyProperty.Register(
            nameof(ItemRowActions),
            typeof(System.Collections.Generic.IList<IGridRowAction>),
            typeof(GridControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="SingleExpandMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SingleExpandModeProperty = DependencyProperty.Register(
            nameof(SingleExpandMode),
            typeof(bool),
            typeof(GridControl),
            new PropertyMetadata(false, OnSingleExpandModeChanged));

        /// <summary>
        /// Identifies the <see cref="ShowSelectionColumn"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowSelectionColumnProperty = DependencyProperty.Register(
            nameof(ShowSelectionColumn),
            typeof(bool),
            typeof(GridControl),
            new PropertyMetadata(false, OnShowSelectionColumnChanged));

        /// <summary>
        /// Identifies the <see cref="IsReadOnly"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(GridControl),
            new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets the data source the grid displays. Setting this rebuilds
        /// the internal <see cref="FlatRowCollection"/> and assigns it to
        /// <see cref="ItemsControl.ItemsSource"/>.
        /// </summary>
        public IGridDataSource DataSource
        {
            get { return (IGridDataSource)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the shared column definitions. Consumers populate this
        /// via <c>&lt;GridControl.Columns&gt;</c> in XAML; the collection is
        /// never <c>null</c>.
        /// </summary>
        public GridColumnCollection Columns
        {
            get { return (GridColumnCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the single selected row. <c>null</c> when nothing is
        /// selected or when the selection contains more than one row.
        /// </summary>
        public IGridRow SelectedRow
        {
            get { return (IGridRow)GetValue(SelectedRowProperty); }
            set { SetValue(SelectedRowProperty, value); }
        }

        /// <summary>
        /// Gets or sets the set of currently selected rows.
        /// </summary>
        public ObservableCollection<IGridRow> SelectedRows
        {
            get { return (ObservableCollection<IGridRow>)GetValue(SelectedRowsProperty); }
            set { SetValue(SelectedRowsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the grid-wide actions rendered by the
        /// <see cref="GridHeaderBar"/> in row 0 of the default template.
        /// </summary>
        public System.Collections.Generic.IList<IGridHeaderAction> HeaderActions
        {
            get { return (System.Collections.Generic.IList<IGridHeaderAction>)GetValue(HeaderActionsProperty); }
            set { SetValue(HeaderActionsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the actions menu entries offered for group rows.
        /// </summary>
        public System.Collections.Generic.IList<IGridRowAction> GroupRowActions
        {
            get { return (System.Collections.Generic.IList<IGridRowAction>)GetValue(GroupRowActionsProperty); }
            set { SetValue(GroupRowActionsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the actions menu entries offered for item rows.
        /// </summary>
        public System.Collections.Generic.IList<IGridRowAction> ItemRowActions
        {
            get { return (System.Collections.Generic.IList<IGridRowAction>)GetValue(ItemRowActionsProperty); }
            set { SetValue(ItemRowActionsProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether only one group may be expanded at a time. When
        /// set to <c>true</c>, any currently expanded groups beyond the first
        /// are collapsed immediately via
        /// <see cref="FlatRowCollection.EnforceSingleExpandMode"/>, and
        /// <see cref="ExpandAllCommand"/> becomes unavailable.
        /// </summary>
        public bool SingleExpandMode
        {
            get { return (bool)GetValue(SingleExpandModeProperty); }
            set { SetValue(SingleExpandModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the built-in <see cref="Columns.SelectionColumn"/>
        /// is shown as the leftmost column. Toggling this inserts the column at
        /// index 0 of <see cref="Columns"/> (or removes the existing one), via
        /// <see cref="OnShowSelectionColumnChanged"/>.
        /// </summary>
        public bool ShowSelectionColumn
        {
            get { return (bool)GetValue(ShowSelectionColumnProperty); }
            set { SetValue(ShowSelectionColumnProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the entire grid is read-only. When <c>true</c>,
        /// no cell can enter edit mode regardless of
        /// <see cref="Columns.GridColumn.IsEditable"/>.
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        /// <summary>
        /// Gets the <see cref="FlatRowCollection"/> backing
        /// <see cref="ItemsControl.ItemsSource"/>, or <c>null</c> if
        /// <see cref="DataSource"/> has not been set.
        /// </summary>
        public FlatRowCollection Rows
        {
            get { return ItemsSource as FlatRowCollection; }
        }

        // -------------------------------------------------------------- //
        //  Change handlers                                                //
        // -------------------------------------------------------------- //

        private static void OnDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (GridControl)d;
            var newSource = (IGridDataSource)e.NewValue;

            grid.ItemsSource = newSource == null ? null : CreateFlatRowCollection(grid, newSource);
        }

        private static FlatRowCollection CreateFlatRowCollection(GridControl grid, IGridDataSource source)
        {
            var flat = new FlatRowCollection(source);
            flat.SingleExpandMode = grid.SingleExpandMode;
            return flat;
        }

        private static void OnShowSelectionColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (GridControl)d;
            var columns = grid.Columns;
            if (columns == null) return;

            if ((bool)e.NewValue)
            {
                // Insert as the leftmost column, unless one is already present.
                if (!HasSelectionColumn(columns))
                    columns.Insert(0, new SelectionColumn());
            }
            else
            {
                for (int i = columns.Count - 1; i >= 0; i--)
                {
                    if (columns[i] is SelectionColumn)
                        columns.RemoveAt(i);
                }
            }
        }

        private static bool HasSelectionColumn(GridColumnCollection columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                if (columns[i] is SelectionColumn)
                    return true;
            }

            return false;
        }

        private static void OnSingleExpandModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (GridControl)d;
            var rows = grid.Rows;
            if (rows == null) return;

            bool enabled = (bool)e.NewValue;
            rows.SingleExpandMode = enabled;
            if (enabled)
                rows.EnforceSingleExpandMode();
        }

        // -------------------------------------------------------------- //
        //  ItemsControl overrides (Task 5.1)                              //
        // -------------------------------------------------------------- //

        /// <inheritdoc/>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is GridRowPresenter;
        }

        /// <inheritdoc/>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new GridRowPresenter { Columns = Columns };
        }

        /// <inheritdoc/>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var presenter = element as GridRowPresenter;
            if (presenter != null)
                presenter.Columns = Columns;
        }
    }
}
