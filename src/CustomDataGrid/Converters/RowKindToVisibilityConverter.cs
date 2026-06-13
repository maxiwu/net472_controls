using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CustomDataGrid.Contracts;

namespace CustomDataGrid.Converters
{
    /// <summary>
    /// One-way converts an <see cref="RowKind"/> to <see cref="Visibility"/>:
    /// <see cref="Visibility.Visible"/> when the value equals the
    /// <see cref="RowKind"/> passed as the converter parameter, otherwise
    /// <see cref="Visibility.Collapsed"/>.
    /// </summary>
    /// <remarks>
    /// Used by the built-in selection column to show its item-row checkbox only
    /// on item rows and its tri-state group-row checkbox only on group rows,
    /// from a single shared cell template.
    /// </remarks>
    public sealed class RowKindToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets the shared instance of this converter, for use as a static
        /// resource reference from XAML.
        /// </summary>
        public static readonly RowKindToVisibilityConverter Instance = new RowKindToVisibilityConverter();

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RowKind kind && parameter is RowKind expected && kind == expected)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
