using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CustomDataGrid.Columns
{
    /// <summary>
    /// A column that renders a <see cref="Button"/> in every cell, sharing a
    /// single <see cref="Command"/> instance across all rows.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Command"/> and <see cref="ButtonContent"/> are defined once at
    /// the column level and reused for every realized cell — never constructed
    /// per row. <see cref="System.Windows.Input.ICommand.Execute"/> receives the
    /// row's <see cref="Contracts.IGridRow"/> as <c>CommandParameter</c>.
    /// </para>
    /// <para>
    /// Clicking the button does not change row selection or highlight — see
    /// <c>GridControl.Mouse.cs</c>'s click classification.
    /// </para>
    /// </remarks>
    public class ButtonColumn : GridColumn
    {
        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(ButtonColumn),
            new PropertyMetadata(null, OnTemplateInputChanged));

        /// <summary>
        /// Identifies the <see cref="ButtonContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register(
            nameof(ButtonContent),
            typeof(object),
            typeof(ButtonColumn),
            new PropertyMetadata(null, OnTemplateInputChanged));

        /// <summary>
        /// Gets or sets the command invoked when the button in any row's cell
        /// is clicked. <c>CommandParameter</c> is the row's
        /// <see cref="Contracts.IGridRow"/>.
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the content (label or icon) shown on the button in
        /// every cell.
        /// </summary>
        public object ButtonContent
        {
            get { return GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }

        static ButtonColumn()
        {
            IsEditableProperty.OverrideMetadata(typeof(ButtonColumn), new PropertyMetadata(false));
        }

        /// <inheritdoc/>
        public override bool SuppressRowSelectionOnClick
        {
            get { return true; }
        }

        private static void OnTemplateInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (ButtonColumn)d;
            column.RebuildTemplate();
        }

        private void RebuildTemplate()
        {
            var factory = new FrameworkElementFactory(typeof(Button));
            factory.SetValue(Button.CommandProperty, Command);
            factory.SetValue(ContentControl.ContentProperty, ButtonContent);
            factory.SetBinding(Button.CommandParameterProperty, new Binding("."));
            factory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            factory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.SetValue(FrameworkElement.MarginProperty, new Thickness(2));

            var template = new DataTemplate();
            template.VisualTree = factory;
            CellTemplate = template;
        }
    }
}
