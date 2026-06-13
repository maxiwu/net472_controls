using System;
using System.Globalization;
using System.Windows.Data;

namespace CustomDataGrid.Converters
{
    /// <summary>
    /// Negates a <see cref="double"/> value. Used to translate the column
    /// headers presenter's <c>GridCellsPanel</c> by the negative of the body
    /// <see cref="System.Windows.Controls.ScrollViewer.HorizontalOffset"/> so
    /// the header tracks horizontal scrolling.
    /// </summary>
    public sealed class NegateDoubleConverter : IValueConverter
    {
        /// <summary>
        /// Gets the shared instance of this converter, for use as a static
        /// resource reference from XAML.
        /// </summary>
        public static readonly NegateDoubleConverter Instance = new NegateDoubleConverter();

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d) return -d;
            return 0.0;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
