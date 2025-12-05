using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ShepherdEplan.Converters
{
    public sealed class StatusToColorConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Colors.White;

            string status = value.ToString()?.Trim().ToLowerInvariant() ?? "";

            return status switch
            {
                "standard" => Colors.LightGreen,
                "forbidden" => Colors.LightCoral,      // Red
                "warning" => Colors.Yellow,
                "notstandard" => Colors.LightBlue,
                "unknown" => Colors.LightGray,
                _ => Colors.White
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}