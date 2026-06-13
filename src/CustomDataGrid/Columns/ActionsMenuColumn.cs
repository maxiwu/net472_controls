using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CustomDataGrid.Contracts;
using CustomDataGrid.Controls;

namespace CustomDataGrid.Columns
{
    /// <summary>
    /// A column whose cell is a trigger button that opens a popup menu of
    /// <see cref="IGridRowAction"/> entries — <see cref="GridControl.GroupRowActions"/>
    /// for <see cref="Models.GridGroupRow"/> rows, <see cref="GridControl.ItemRowActions"/>
    /// for <see cref="Models.GridItemRow"/> rows.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Lazy, reused popup.</b> The popup is built on first click for a given
    /// cell and reused for that cell's container on subsequent opens; its
    /// <see cref="FrameworkElement.DataContext"/> is reset to the current row
    /// each time it opens. The popup content is never instantiated for
    /// non-clicked cells.
    /// </para>
    /// <para>
    /// <b>CanExecute only on open.</b> Each action's
    /// <see cref="System.Windows.Input.ICommand.CanExecute(object)"/> is
    /// evaluated once, when the popup opens, to set that menu item's
    /// <see cref="Control.IsEnabled"/>. No per-row binding to <c>CanExecute</c>
    /// is created — at scale, <c>CommandManager.RequerySuggested</c> would turn
    /// such a binding into a full-row-count walk on every requery.
    /// </para>
    /// <para>
    /// If <see cref="ActionsMenuTemplate"/> is set, it replaces the default
    /// popup content; its <c>DataContext</c> is still the current row.
    /// </para>
    /// <para>
    /// Clicking the trigger button does not change row selection or highlight.
    /// </para>
    /// </remarks>
    public class ActionsMenuColumn : GridColumn
    {
        /// <summary>
        /// Identifies the <see cref="ActionsMenuTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ActionsMenuTemplateProperty = DependencyProperty.Register(
            nameof(ActionsMenuTemplate),
            typeof(DataTemplate),
            typeof(ActionsMenuColumn),
            new PropertyMetadata(null));

        static ActionsMenuColumn()
        {
            IsEditableProperty.OverrideMetadata(typeof(ActionsMenuColumn), new PropertyMetadata(false));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionsMenuColumn"/> class.
        /// </summary>
        public ActionsMenuColumn()
        {
            CellTemplate = BuildCellTemplate();
        }

        /// <summary>
        /// Gets or sets a template that replaces the default popup content. The
        /// template's <see cref="FrameworkElement.DataContext"/> is the current
        /// row (an <see cref="IGridRow"/>).
        /// </summary>
        public DataTemplate ActionsMenuTemplate
        {
            get { return (DataTemplate)GetValue(ActionsMenuTemplateProperty); }
            set { SetValue(ActionsMenuTemplateProperty, value); }
        }

        /// <inheritdoc/>
        public override bool SuppressRowSelectionOnClick
        {
            get { return true; }
        }

        private static DataTemplate BuildCellTemplate()
        {
            var factory = new FrameworkElementFactory(typeof(Button));
            factory.SetValue(ContentControl.ContentProperty, "⋯"); // horizontal ellipsis
            factory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            factory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.SetValue(FrameworkElement.MarginProperty, new Thickness(2));
            factory.AddHandler(ButtonBase.ClickEvent, (RoutedEventHandler)OnTriggerClick);

            var template = new DataTemplate();
            template.VisualTree = factory;
            return template;
        }

        private static void OnTriggerClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var row = button.DataContext as IGridRow;
            if (row == null) return;

            var grid = EditingElementHelper.FindAncestorGridControl(button);
            if (grid == null) return;

            var column = EditingElementHelper.ResolveColumn(grid, button) as ActionsMenuColumn;
            if (column == null) return;

            var popup = GetOrCreatePopup(button, column);
            PopulatePopup(popup, grid, column, row);

            popup.DataContext = row;
            popup.PlacementTarget = button;
            popup.IsOpen = true;
        }

        private static readonly DependencyProperty PopupProperty = DependencyProperty.RegisterAttached(
            "Popup",
            typeof(Popup),
            typeof(ActionsMenuColumn),
            new PropertyMetadata(null));

        private static Popup GetOrCreatePopup(Button button, ActionsMenuColumn column)
        {
            var popup = (Popup)button.GetValue(PopupProperty);
            if (popup != null) return popup;

            popup = new Popup
            {
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                AllowsTransparency = true
            };

            button.SetValue(PopupProperty, popup);
            return popup;
        }

        private static void PopulatePopup(Popup popup, GridControl grid, ActionsMenuColumn column, IGridRow row)
        {
            if (column.ActionsMenuTemplate != null)
            {
                var presenter = new ContentPresenter
                {
                    ContentTemplate = column.ActionsMenuTemplate,
                    Content = row
                };
                popup.Child = presenter;
                return;
            }

            var actions = row.Kind == RowKind.Group ? grid.GroupRowActions : grid.ItemRowActions;

            var menu = new ContextMenu();
            if (actions != null)
            {
                foreach (var action in actions)
                    menu.Items.Add(BuildMenuItem(action, row));
            }

            // Host the ContextMenu inside the popup so it participates in normal
            // popup placement/closing rather than the separate context-menu
            // open gesture.
            menu.IsOpen = true;
            popup.Child = menu;
            popup.Closed += (s, e) => menu.IsOpen = false;
        }

        private static Control BuildMenuItem(IGridRowAction action, IGridRow row)
        {
            if (action.IsSeparator)
                return new Separator();

            var item = new MenuItem
            {
                Header = action.Label,
                Icon = action.Icon != null ? new Image { Source = action.Icon, Width = 16, Height = 16 } : null,
                Command = action.Command,
                CommandParameter = row,
                IsEnabled = action.Command == null || action.Command.CanExecute(row)
            };

            return item;
        }
    }
}
