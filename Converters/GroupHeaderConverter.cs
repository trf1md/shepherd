using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ShepherdEplan.Converters
{
    public sealed class GroupHeaderConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value = CurrentGroupBy (e.g., "Location")
            // parameter = this header's property name (e.g., "Location")

            if (value is string currentGroup && parameter is string headerName)
            {
                // If this header is the active group, highlight it
                if (currentGroup == headerName)
                    return Colors.LightBlue; // Highlighted color
            }

            return Colors.Transparent; // Default (no highlight)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}