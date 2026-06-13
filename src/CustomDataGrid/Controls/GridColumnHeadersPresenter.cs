using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CustomDataGrid.Columns;

namespace CustomDataGrid.Controls
{
    /// <summary>
    /// The column-headers row of a <see cref="GridControl"/>. Lives in row 1 of
    /// the default control template — outside the body
    /// <see cref="ScrollViewer"/> — so it does not scroll vertically with the
    /// rows.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Internally hosts a <see cref="GridCellsPanel"/> bound to the same
    /// <see cref="Columns"/> collection as every row's cells panel, so headers
    /// and rows align using the shared-width model described in design doc §5.4
    /// (no <c>Grid.IsSharedSizeScope</c> / <c>SharedSizeGroup</c>).
    /// </para>
    /// <para>
    /// <see cref="HorizontalOffset"/> is bound to the body
    /// <see cref="ScrollViewer.HorizontalOffset"/> and applied as a negative
    /// <see cref="TranslateTransform.X"/> on the internal panel so the header
    /// tracks horizontal scroll.
    /// </para>
    /// </remarks>
    public class GridColumnHeadersPresenter : Control
    {
        /// <summary>
        /// Identifies the <see cref="Columns"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            nameof(Columns),
            typeof(GridColumnCollection),
            typeof(GridColumnHeadersPresenter),
            new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="HorizontalOffset"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register(
            nameof(HorizontalOffset),
            typeof(double),
            typeof(GridColumnHeadersPresenter),
            new PropertyMetadata(0d));

        static GridColumnHeadersPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GridColumnHeadersPresenter),
                new FrameworkPropertyMetadata(typeof(GridColumnHeadersPresenter)));
        }

        /// <summary>
        /// Gets or sets the shared column collection. Typically bound to
        /// <c>GridControl.Columns</c>.
        /// </summary>
        public GridColumnCollection Columns
        {
            get { return (GridColumnCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the horizontal offset applied to the header content,
        /// bound to the body <see cref="ScrollViewer"/>'s
        /// <see cref="ScrollViewer.HorizontalOffset"/> so the header tracks
        /// horizontal scroll of the rows beneath it.
        /// </summary>
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }
    }
}
