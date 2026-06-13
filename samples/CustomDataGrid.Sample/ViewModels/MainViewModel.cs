using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using CustomDataGrid.Columns;
using CustomDataGrid.Commands;
using CustomDataGrid.Contracts;
using CustomDataGrid.Contracts.Events;
using CustomDataGrid.Models;
using CustomDataGrid.Sample.Models;

namespace CustomDataGrid.Sample.ViewModels
{
    /// <summary>
    /// The sample's main view model (Tasks 8.2 / 8.4 / 8.6): owns the data
    /// source, columns, header / row actions, the toggle commands, and the event
    /// log. Routed grid events are forwarded here from the window's code-behind
    /// and translated into <see cref="LogViewModel"/> entries.
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        private readonly SampleDataSource _dataSource;
        private bool _singleExpandMode;
        private bool _showSelectionColumn = true;
        private IGridRow _selectedRow;

        /// <summary>
        /// Initializes the view model, building the data source, columns, and
        /// actions.
        /// </summary>
        public MainViewModel()
        {
            _dataSource = new SampleDataSource();

            Columns = BuildColumns();
            HeaderActions = BuildHeaderActions();
            GroupRowActions = BuildGroupActions();
            ItemRowActions = BuildItemActions();

            ToggleSingleExpandModeCommand = new RelayCommand(p => SingleExpandMode = !SingleExpandMode);
            ToggleShowSelectionColumnCommand = new RelayCommand(p => ShowSelectionColumn = !ShowSelectionColumn);
            ClearLogCommand = new RelayCommand(p => Log.Clear());
        }

        // -------------------------------------------------------------- //
        //  Grid-bound state                                               //
        // -------------------------------------------------------------- //

        /// <summary>Gets the grid's data source.</summary>
        public IGridDataSource DataSource { get { return _dataSource; } }

        /// <summary>Gets the shared column definitions.</summary>
        public GridColumnCollection Columns { get; }

        /// <summary>Gets the header toolbar actions.</summary>
        public IList<IGridHeaderAction> HeaderActions { get; }

        /// <summary>Gets the group-row actions-menu entries.</summary>
        public IList<IGridRowAction> GroupRowActions { get; }

        /// <summary>Gets the item-row actions-menu entries.</summary>
        public IList<IGridRowAction> ItemRowActions { get; }

        /// <summary>Gets or sets the grid's single selected row (two-way bound).</summary>
        public IGridRow SelectedRow
        {
            get { return _selectedRow; }
            set { SetField(ref _selectedRow, value); }
        }

        /// <summary>Gets the grid's multi-selection (two-way bound).</summary>
        public ObservableCollection<IGridRow> SelectedRows { get; } = new ObservableCollection<IGridRow>();

        /// <summary>Gets or sets whether only one group may be expanded at a time.</summary>
        public bool SingleExpandMode
        {
            get { return _singleExpandMode; }
            set { SetField(ref _singleExpandMode, value); }
        }

        /// <summary>Gets or sets whether the built-in selection column is shown.</summary>
        public bool ShowSelectionColumn
        {
            get { return _showSelectionColumn; }
            set { SetField(ref _showSelectionColumn, value); }
        }

        // -------------------------------------------------------------- //
        //  Commands & log                                                 //
        // -------------------------------------------------------------- //

        /// <summary>Gets the event log.</summary>
        public LogViewModel Log { get; } = new LogViewModel();

        /// <summary>Toggles <see cref="SingleExpandMode"/>.</summary>
        public ICommand ToggleSingleExpandModeCommand { get; }

        /// <summary>Toggles <see cref="ShowSelectionColumn"/>.</summary>
        public ICommand ToggleShowSelectionColumnCommand { get; }

        /// <summary>Clears the event log.</summary>
        public ICommand ClearLogCommand { get; }

        // -------------------------------------------------------------- //
        //  Columns (Task 8.2)                                             //
        // -------------------------------------------------------------- //

        private static GridColumnCollection BuildColumns()
        {
            var columns = new GridColumnCollection();

            // Group-row columns (render blank on item rows).
            columns.Add(new TextColumn
            {
                Header = "Description",
                ColumnName = "Description",
                Width = new System.Windows.GridLength(2, System.Windows.GridUnitType.Star),
                Binding = new Binding("Description") { Mode = BindingMode.TwoWay }
            });
            columns.Add(new ComboBoxColumn
            {
                Header = "Status",
                ColumnName = "Status",
                Width = new System.Windows.GridLength(110),
                ItemsSource = new[] { "Enable", "Disable" },
                Binding = new Binding("Status") { Mode = BindingMode.TwoWay }
            });

            // Item-row columns (render blank on group rows).
            columns.Add(new TextColumn
            {
                Header = "X",
                ColumnName = "X",
                Width = new System.Windows.GridLength(80),
                Binding = new Binding("X") { Mode = BindingMode.TwoWay, StringFormat = "0.00" }
            });
            columns.Add(new TextColumn
            {
                Header = "Y",
                ColumnName = "Y",
                Width = new System.Windows.GridLength(80),
                Binding = new Binding("Y") { Mode = BindingMode.TwoWay, StringFormat = "0.00" }
            });

            // Actions menu (resolves group vs item actions from the grid).
            columns.Add(new ActionsMenuColumn
            {
                Header = "",
                Width = new System.Windows.GridLength(44)
            });

            return columns;
        }

        // -------------------------------------------------------------- //
        //  Header actions (Task 8.6)                                      //
        // -------------------------------------------------------------- //

        private IList<IGridHeaderAction> BuildHeaderActions()
        {
            var addGroup = new GridHeaderAction("Add Group", new RelayCommand(p => AddGroup()));
            var deleteSelected = new GridHeaderAction("Delete Selected", new RelayCommand(p => DeleteSelected()));
            return new List<IGridHeaderAction> { addGroup, deleteSelected };
        }

        private void AddGroup()
        {
            var group = _dataSource.CreateGroup();
            _dataSource.AddGroup(_dataSource.GroupCount, group);
            Log.AddEntry(LogCategory.Action, "New group added: " + group.Label);
        }

        private void DeleteSelected()
        {
            int groups = 0;
            int items = 0;

            // Snapshot the selection; deleting mutates SelectedRows.
            var snapshot = new List<IGridRow>(SelectedRows);
            foreach (var row in snapshot)
            {
                var item = row as GridItemRow;
                if (item != null && TryDeleteItem(item))
                {
                    items++;
                    continue;
                }

                var group = row as GridGroupRow;
                if (group != null && TryDeleteGroup(group))
                    groups++;
            }

            Log.AddEntry(LogCategory.Action,
                "Deleted selected: " + groups + " groups, " + items + " items");
        }

        // -------------------------------------------------------------- //
        //  Row actions (Task 8.4)                                         //
        // -------------------------------------------------------------- //

        private IList<IGridRowAction> BuildGroupActions()
        {
            return new List<IGridRowAction>
            {
                new GridRowAction("Copy", new RelayCommand(p => LogRowAction("Copy", p))),
                new GridRowAction("Modify", new RelayCommand(p => LogRowAction("Modify", p)))
            };
        }

        private IList<IGridRowAction> BuildItemActions()
        {
            return new List<IGridRowAction>
            {
                new GridRowAction("Copy", new RelayCommand(p => LogRowAction("Copy", p))),
                new GridRowAction("Modify", new RelayCommand(p => LogRowAction("Modify", p))),
                new GridRowAction("Delete", new RelayCommand(p => DeleteRowAction(p)))
            };
        }

        private void LogRowAction(string action, object parameter)
        {
            Log.AddEntry(LogCategory.Action, action + ": " + Describe(parameter as IGridRow));
        }

        private void DeleteRowAction(object parameter)
        {
            var item = parameter as GridItemRow;
            if (item != null && TryDeleteItem(item))
                Log.AddEntry(LogCategory.Action, "Delete: " + Describe(item));
        }

        private bool TryDeleteItem(GridItemRow item)
        {
            for (int g = 0; g < _dataSource.GroupCount; g++)
            {
                var group = _dataSource.GetGroup(g);
                int idx = group.Items.IndexOf(item);
                if (idx >= 0)
                {
                    _dataSource.RemoveItem(g, idx);
                    SelectedRows.Remove(item);
                    return true;
                }
            }

            return false;
        }

        private bool TryDeleteGroup(GridGroupRow group)
        {
            for (int g = 0; g < _dataSource.GroupCount; g++)
            {
                if (ReferenceEquals(_dataSource.GetGroup(g), group))
                {
                    _dataSource.RemoveGroup(g);
                    SelectedRows.Remove(group);
                    return true;
                }
            }

            return false;
        }

        // -------------------------------------------------------------- //
        //  Routed-event translation (Task 8.6)                            //
        // -------------------------------------------------------------- //

        /// <summary>Logs a <c>SelectedRowChanged</c> routed event.</summary>
        public void OnSelectedRowChanged(SelectedRowChangedEventArgs e)
        {
            if (e.NewRow != null)
                Log.AddEntry(LogCategory.Selection, "Row selected: " + Describe(e.NewRow));
            else
                Log.AddEntry(LogCategory.Selection, "Selection cleared");
        }

        /// <summary>Logs a <c>SelectedRowsChanged</c> routed event.</summary>
        public void OnSelectedRowsChanged(SelectedRowsChangedEventArgs e)
        {
            int added = e.AddedRows != null ? e.AddedRows.Count : 0;
            int removed = e.RemovedRows != null ? e.RemovedRows.Count : 0;
            Log.AddEntry(LogCategory.Selection,
                SelectedRows.Count + " rows selected (+" + added + "/-" + removed + ")");
        }

        /// <summary>Logs a <c>GroupExpanded</c> routed event.</summary>
        public void OnGroupExpanded(GroupExpandedEventArgs e)
        {
            Log.AddEntry(LogCategory.Action, "Group expanded: " + Describe(e.Group));
        }

        /// <summary>Logs a <c>GroupCollapsed</c> routed event.</summary>
        public void OnGroupCollapsed(GroupCollapsedEventArgs e)
        {
            Log.AddEntry(LogCategory.Action, "Group collapsed: " + Describe(e.Group));
        }

        /// <summary>Logs a <c>CellEditCommitted</c> routed event.</summary>
        public void OnCellEditCommitted(CellEditCommittedEventArgs e)
        {
            Log.AddEntry(LogCategory.Edit,
                Describe(e.Row) + " -> " + e.ColumnName + " changed: "
                + Format(e.OldValue) + " -> " + Format(e.NewValue));
        }

        /// <summary>Logs a <c>CellEditCancelled</c> routed event.</summary>
        public void OnCellEditCancelled(CellEditCancelledEventArgs e)
        {
            Log.AddEntry(LogCategory.Edit,
                "Edit cancelled: " + Describe(e.Row) + " / " + e.ColumnName);
        }

        // -------------------------------------------------------------- //
        //  Helpers                                                        //
        // -------------------------------------------------------------- //

        private static string Describe(IGridRow row)
        {
            var group = row as GridGroupRow;
            if (group != null) return group.Label;

            var item = row as SampleItemRow;
            if (item != null)
                return "Item (" + item.X.ToString("0.00", CultureInfo.InvariantCulture)
                    + ", " + item.Y.ToString("0.00", CultureInfo.InvariantCulture) + ")";

            return row != null ? row.ToString() : "(null)";
        }

        private static string Format(object value)
        {
            if (value == null) return "(null)";
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
}
