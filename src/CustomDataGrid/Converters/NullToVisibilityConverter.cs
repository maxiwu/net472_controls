using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CustomDataGrid.Converters
{
    /// <summary>
    /// Converts <c>null</c> to <see cref="Visibility.Collapsed"/> and any
    /// non-null value to <see cref="Visibility.Visible"/>. Used to hide an
    /// optional icon image when no <see cref="System.Windows.Media.ImageSource"/>
    /// is supplied.
    /// </summary>
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets the shared instance of this converter, for use as a static
        /// resource reference from XAML.
        /// </summary>
        public static readonly NullToVisibilityConverter Instance = new NullToVisibilityConverter();

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
