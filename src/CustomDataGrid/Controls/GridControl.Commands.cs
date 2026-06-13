using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CustomDataGrid.Commands;
using CustomDataGrid.Contracts;
using CustomDataGrid.Models;

namespace CustomDataGrid.Controls
{
    /// <summary>
    /// Inbound command dependency properties (Task 5.4). Each command's
    /// <see cref="ICommand.CanExecute"/> / <see cref="ICommand.Execute"/> is
    /// invoked by the control in response to user gestures; consumers may also
    /// invoke them directly (e.g. <see cref="ScrollToRowCommand"/> from a
    /// "find" feature).
    /// </summary>
    public partial class GridControl
    {
        /// <summary>
        /// Identifies the <see cref="ScrollToRowCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScrollToRowCommandProperty = DependencyProperty.Register(
            nameof(ScrollToRowCommand),
            typeof(ICommand),
            typeof(GridControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="SelectRowCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectRowCommandProperty = DependencyProperty.Register(
            nameof(SelectRowCommand),
            typeof(ICommand),
            typeof(GridControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ExpandGroupCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ExpandGroupCommandProperty = DependencyProperty.Register(
            nameof(ExpandGroupCommand),
            typeof(ICommand),
            typeof(GridControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="CollapseGroupCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CollapseGroupCommandProperty = DependencyProperty.Register(
            nameof(CollapseGroupCommand),
            typeof(ICommand),
            typeof(GridControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="CollapseAllCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CollapseAllCommandProperty = DependencyProperty.Register(
            nameof(CollapseAllCommand),
            typeof(ICommand),
            typeof(GridControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ExpandAllCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ExpandAllCommandProperty = DependencyProperty.Register(
            nameof(ExpandAllCommand),
            typeof(ICommand),
            typeof(GridControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="BeginEditRowCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BeginEditRowCommandProperty = DependencyProperty.Register(
            nameof(BeginEditRowCommand),
            typeof(ICommand),
            typeof(GridControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the command that scrolls the body
        /// <see cref="ScrollViewer"/> to bring a row into view. The command
        /// parameter is the <see cref="IGridRow"/> to scroll to. Implemented by
        /// resolving the row's flat index via <see cref="ItemContainerGenerator"/>
        /// and calling <see cref="VirtualizingPanel"/> item-mode scrolling — there
        /// is no DataGrid <c>ScrollIntoView</c> available.
        /// </summary>
        public ICommand ScrollToRowCommand
        {
            get { return (ICommand)GetValue(ScrollToRowCommandProperty); }
            set { SetValue(ScrollToRowCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the command that selects a row. The command parameter is
        /// the <see cref="IGridRow"/> to select. No-ops when the row's
        /// <see cref="IGridRow.IsEnabled"/> is <c>false</c>.
        /// </summary>
        public ICommand SelectRowCommand
        {
            get { return (ICommand)GetValue(SelectRowCommandProperty); }
            set { SetValue(SelectRowCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the command that expands a group. The command parameter
        /// is the <see cref="GridGroupRow"/> to expand. No-ops when the group's
        /// <see cref="IGridRow.IsEnabled"/> is <c>false</c>.
        /// </summary>
        public ICommand ExpandGroupCommand
        {
            get { return (ICommand)GetValue(ExpandGroupCommandProperty); }
            set { SetValue(ExpandGroupCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the command that collapses a group. The command
        /// parameter is the <see cref="GridGroupRow"/> to collapse. No-ops when
        /// the group's <see cref="IGridRow.IsEnabled"/> is <c>false</c>.
        /// </summary>
        public ICommand CollapseGroupCommand
        {
            get { return (ICommand)GetValue(CollapseGroupCommandProperty); }
            set { SetValue(CollapseGroupCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the command that collapses every currently expanded
        /// group.
        /// </summary>
        public ICommand CollapseAllCommand
        {
            get { return (ICommand)GetValue(CollapseAllCommandProperty); }
            set { SetValue(CollapseAllCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the command that expands every group.
        /// <see cref="ICommand.CanExecute"/> returns <c>false</c> when
        /// <see cref="SingleExpandMode"/> is <c>true</c>.
        /// </summary>
        public ICommand ExpandAllCommand
        {
            get { return (ICommand)GetValue(ExpandAllCommandProperty); }
            set { SetValue(ExpandAllCommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the command that begins editing the first editable cell
        /// of a row. The command parameter is the <see cref="IGridRow"/> to
        /// edit. No-ops when the row's <see cref="IGridRow.IsEnabled"/> is
        /// <c>false</c>.
        /// </summary>
        public ICommand BeginEditRowCommand
        {
            get { return (ICommand)GetValue(BeginEditRowCommandProperty); }
            set { SetValue(BeginEditRowCommandProperty, value); }
        }

        /// <summary>
        /// Expands the group at <paramref name="groupIndex"/> via the internal
        /// <see cref="FlatRowCollection"/> and raises <see cref="GroupExpanded"/>.
        /// No-ops if the group is disabled or already expanded.
        /// </summary>
        /// <param name="groupIndex">The zero-based index of the group to expand.</param>
        public void ExpandGroup(int groupIndex)
        {
            if (groupIndex < 0) return;

            var rows = Rows;
            if (rows == null) return;

            var source = DataSource;
            var group = source.GetGroup(groupIndex);
            if (!group.IsEnabled || group.IsExpanded) return;

            rows.SetExpanded(groupIndex, true);
            RaiseGroupExpanded(group, groupIndex);
        }

        /// <summary>
        /// Collapses the group at <paramref name="groupIndex"/> via the internal
        /// <see cref="FlatRowCollection"/> and raises <see cref="GroupCollapsed"/>.
        /// No-ops if the group is disabled or already collapsed.
        /// </summary>
        /// <param name="groupIndex">The zero-based index of the group to collapse.</param>
        public void CollapseGroup(int groupIndex)
        {
            if (groupIndex < 0) return;

            var rows = Rows;
            if (rows == null) return;

            var source = DataSource;
            var group = source.GetGroup(groupIndex);
            if (!group.IsEnabled || !group.IsExpanded) return;

            rows.SetExpanded(groupIndex, false);
            RaiseGroupCollapsed(group, groupIndex);
        }

        /// <summary>
        /// Collapses every currently expanded, enabled group.
        /// </summary>
        public void CollapseAll()
        {
            var source = DataSource;
            if (source == null) return;

            for (int i = 0; i < source.GroupCount; i++)
            {
                var group = source.GetGroup(i);
                if (group.IsEnabled && group.IsExpanded)
                    CollapseGroup(i);
            }
        }

        /// <summary>
        /// Expands every enabled group. No-ops when
        /// <see cref="SingleExpandMode"/> is <c>true</c>.
        /// </summary>
        public void ExpandAll()
        {
            if (SingleExpandMode) return;

            var source = DataSource;
            if (source == null) return;

            for (int i = 0; i < source.GroupCount; i++)
            {
                var group = source.GetGroup(i);
                if (group.IsEnabled && !group.IsExpanded)
                    ExpandGroup(i);
            }
        }

        /// <summary>
        /// Selects <paramref name="row"/>, replacing the current single
        /// selection. No-ops when the row's <see cref="IGridRow.IsEnabled"/> is
        /// <c>false</c>.
        /// </summary>
        /// <param name="row">The row to select.</param>
        public void SelectRow(IGridRow row)
        {
            if (row == null || !row.IsEnabled) return;
            SetSingleSelection(row);
        }

        /// <summary>
        /// Begins editing the first editable, enabled column of <paramref name="row"/>.
        /// No-ops when the row is disabled, the grid is read-only, or no column
        /// is editable.
        /// </summary>
        /// <param name="row">The row to edit.</param>
        public void BeginEditRow(IGridRow row)
        {
            if (row == null || !row.IsEnabled || IsReadOnly) return;

            foreach (var column in Columns)
            {
                if (column.IsEditable)
                {
                    BeginEdit(row, column, null);
                    return;
                }
            }
        }

        /// <summary>
        /// Scrolls the body <see cref="ScrollViewer"/> so that
        /// <paramref name="row"/> is brought into view, using item-mode
        /// scrolling against the row's realized container.
        /// </summary>
        /// <param name="row">The row to scroll to.</param>
        public void ScrollToRow(IGridRow row)
        {
            if (row == null) return;

            var container = ItemContainerGenerator.ContainerFromItem(row) as FrameworkElement;
            if (container != null)
                container.BringIntoView();
        }

        /// <summary>
        /// Assigns default <see cref="RelayCommand"/> implementations to the
        /// inbound command DPs that have not been set by the consumer, wiring
        /// them to this control's selection / expand / edit methods.
        /// </summary>
        private void InitializeCommands()
        {
            ScrollToRowCommand = new RelayCommand(p => ScrollToRow(p as IGridRow));

            SelectRowCommand = new RelayCommand(p => SelectRow(p as IGridRow));

            ExpandGroupCommand = new RelayCommand(p =>
            {
                var group = p as GridGroupRow;
                if (group != null) ExpandGroup(IndexOfGroup(group));
            });

            CollapseGroupCommand = new RelayCommand(p =>
            {
                var group = p as GridGroupRow;
                if (group != null) CollapseGroup(IndexOfGroup(group));
            });

            CollapseAllCommand = new RelayCommand(p => CollapseAll());

            ExpandAllCommand = new RelayCommand(
                p => ExpandAll(),
                p => !SingleExpandMode);

            BeginEditRowCommand = new RelayCommand(p => BeginEditRow(p as IGridRow));
        }

        /// <summary>
        /// Finds the zero-based index of <paramref name="group"/> within
        /// <see cref="DataSource"/> by reference equality.
        /// </summary>
        /// <param name="group">The group to locate.</param>
        /// <returns>The group's index, or -1 if not found.</returns>
        private int IndexOfGroup(GridGroupRow group)
        {
            var source = DataSource;
            if (source == null) return -1;

            for (int i = 0; i < source.GroupCount; i++)
            {
                if (ReferenceEquals(source.GetGroup(i), group))
                    return i;
            }

            return -1;
        }
    }
}
