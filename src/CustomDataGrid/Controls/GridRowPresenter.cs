using System.Windows;
using System.Windows.Controls;
using CustomDataGrid.Columns;
using CustomDataGrid.Contracts;

namespace CustomDataGrid.Controls
{
    /// <summary>
    /// The item container <see cref="GridControl"/> returns from
    /// <see cref="GridControl.GetContainerForItemOverride"/>. Its
    /// <see cref="FrameworkElement.DataContext"/> is the <see cref="IGridRow"/>
    /// (a <c>GridGroupRow</c> or <c>GridItemRow</c>) for the row it represents.
    /// </summary>
    /// <remarks>
    /// The default template hosts a <see cref="GridCellsPanel"/> plus a
    /// full-row background element used to render highlight, disabled, and
    /// selection visual states (see <c>Themes/Generic.xaml</c> and the row /
    /// cell style selectors in Phase 7).
    /// </remarks>
    public class GridRowPresenter : ContentControl
    {
        /// <summary>
        /// Identifies the <see cref="Columns"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            nameof(Columns),
            typeof(GridColumnCollection),
            typeof(GridRowPresenter),
            new PropertyMetadata(null));

        static GridRowPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GridRowPresenter),
                new FrameworkPropertyMetadata(typeof(GridRowPresenter)));
        }

        /// <summary>
        /// Gets or sets the shared column collection passed down to this row's
        /// <see cref="GridCellsPanel"/>. Set by <see cref="GridControl"/> when the
        /// container is generated.
        /// </summary>
        public GridColumnCollection Columns
        {
            get { return (GridColumnCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// Gets the row data this container represents.
        /// </summary>
        public IGridRow Row
        {
            get { return DataContext as IGridRow; }
        }
    }
}
