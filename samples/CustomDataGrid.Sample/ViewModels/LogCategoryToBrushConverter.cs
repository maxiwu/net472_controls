using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CustomDataGrid.Sample.ViewModels
{
    /// <summary>
    /// Maps a <see cref="LogCategory"/> to its badge color (Task 8.7):
    /// SELECTION = blue, EDIT = orange, ACTION = red.
    /// </summary>
    public sealed class LogCategoryToBrushConverter : IValueConverter
    {
        /// <summary>Gets the shared instance, for use as a static resource.</summary>
        public static readonly LogCategoryToBrushConverter Instance = new LogCategoryToBrushConverter();

        private static readonly Brush SelectionBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x1E, 0x6F, 0xD9)));
        private static readonly Brush EditBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xE8, 0x7A, 0x17)));
        private static readonly Brush ActionBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xD1, 0x3A, 0x3A)));
        private static readonly Brush DefaultBrush = Freeze(new SolidColorBrush(Colors.Gray));

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LogCategory category)
            {
                switch (category)
                {
                    case LogCategory.Selection: return SelectionBrush;
                    case LogCategory.Edit: return EditBrush;
                    case LogCategory.Action: return ActionBrush;
                }
            }

            return DefaultBrush;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static Brush Freeze(SolidColorBrush brush)
        {
            brush.Freeze();
            return brush;
        }
    }
}
