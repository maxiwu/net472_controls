using System.Windows;

namespace CustomDataGrid.Controls
{
    /// <summary>
    /// Attached properties carrying a data cell's visual state, set by
    /// <see cref="GridCellsPanel"/> on each cell <see cref="System.Windows.Controls.ContentControl"/>
    /// and consumed by the cell <c>Style</c> triggers in <c>Themes/Generic.xaml</c>.
    /// </summary>
    /// <remarks>
    /// These exist because cell visual states (disabled / highlighted / editing)
    /// change <em>after</em> a cell container is created — when the user selects a
    /// row, disables a group, or begins an edit — and must react to those changes.
    /// A one-shot <see cref="System.Windows.Controls.StyleSelector"/> cannot, and a
    /// cell <see cref="System.Windows.Controls.ContentControl"/> exposes no built-in
    /// row-disabled / highlighted / editing properties for triggers to bind to.
    /// <see cref="GridCellsPanel"/> owns the knowledge (the row, the owning-group
    /// disabled cascade, and the grid's edit state) and pushes it onto each cell via
    /// these properties. See design doc §5.8 / Task 7.2.
    /// </remarks>
    public static class GridCell
    {
        /// <summary>
        /// Identifies the <c>GridCell.IsRowDisabled</c> attached property:
        /// <c>true</c> when the cell's row is disabled — the row's
        /// <see cref="Contracts.IGridRow.IsEnabled"/> is <c>false</c> or its
        /// owning group is disabled.
        /// </summary>
        public static readonly DependencyProperty IsRowDisabledProperty = DependencyProperty.RegisterAttached(
            "IsRowDisabled",
            typeof(bool),
            typeof(GridCell),
            new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <c>GridCell.IsRowHighlighted</c> attached property:
        /// mirrors the cell's row <see cref="Contracts.IGridRow.IsHighlighted"/>.
        /// </summary>
        public static readonly DependencyProperty IsRowHighlightedProperty = DependencyProperty.RegisterAttached(
            "IsRowHighlighted",
            typeof(bool),
            typeof(GridCell),
            new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <c>GridCell.IsEditing</c> attached property:
        /// <c>true</c> while this specific cell is in edit mode.
        /// </summary>
        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.RegisterAttached(
            "IsEditing",
            typeof(bool),
            typeof(GridCell),
            new PropertyMetadata(false));

        /// <summary>Gets the <c>GridCell.IsRowDisabled</c> value.</summary>
        /// <param name="element">The cell element.</param>
        public static bool GetIsRowDisabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsRowDisabledProperty);
        }

        /// <summary>Sets the <c>GridCell.IsRowDisabled</c> value.</summary>
        /// <param name="element">The cell element.</param>
        /// <param name="value">The value to set.</param>
        public static void SetIsRowDisabled(DependencyObject element, bool value)
        {
            element.SetValue(IsRowDisabledProperty, value);
        }

        /// <summary>Gets the <c>GridCell.IsRowHighlighted</c> value.</summary>
        /// <param name="element">The cell element.</param>
        public static bool GetIsRowHighlighted(DependencyObject element)
        {
            return (bool)element.GetValue(IsRowHighlightedProperty);
        }

        /// <summary>Sets the <c>GridCell.IsRowHighlighted</c> value.</summary>
        /// <param name="element">The cell element.</param>
        /// <param name="value">The value to set.</param>
        public static void SetIsRowHighlighted(DependencyObject element, bool value)
        {
            element.SetValue(IsRowHighlightedProperty, value);
        }

        /// <summary>Gets the <c>GridCell.IsEditing</c> value.</summary>
        /// <param name="element">The cell element.</param>
        public static bool GetIsEditing(DependencyObject element)
        {
            return (bool)element.GetValue(IsEditingProperty);
        }

        /// <summary>Sets the <c>GridCell.IsEditing</c> value.</summary>
        /// <param name="element">The cell element.</param>
        /// <param name="value">The value to set.</param>
        public static void SetIsEditing(DependencyObject element, bool value)
        {
            element.SetValue(IsEditingProperty, value);
        }
    }
}
