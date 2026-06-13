using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CustomDataGrid.Contracts;

namespace CustomDataGrid.Controls
{
    /// <summary>
    /// The grid-wide action toolbar, rendered in row 0 of
    /// <see cref="GridControl"/>'s default control template, independent of the
    /// body. Each button is bound to an <see cref="IGridHeaderAction"/>: its
    /// <c>Label</c>, <c>Command</c>, <c>IsEnabled</c>, and optional <c>Icon</c>.
    /// </summary>
    public partial class GridHeaderBar : UserControl
    {
        /// <summary>
        /// Identifies the <see cref="HeaderActions"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderActionsProperty = DependencyProperty.Register(
            nameof(HeaderActions),
            typeof(IList<IGridHeaderAction>),
            typeof(GridHeaderBar),
            new PropertyMetadata(null));

        /// <summary>
        /// Initializes a new instance of the <see cref="GridHeaderBar"/> class.
        /// </summary>
        public GridHeaderBar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the actions rendered as right-aligned buttons in this
        /// toolbar. Typically bound to <c>GridControl.HeaderActions</c>.
        /// </summary>
        public IList<IGridHeaderAction> HeaderActions
        {
            get { return (IList<IGridHeaderAction>)GetValue(HeaderActionsProperty); }
            set { SetValue(HeaderActionsProperty, value); }
        }
    }
}
