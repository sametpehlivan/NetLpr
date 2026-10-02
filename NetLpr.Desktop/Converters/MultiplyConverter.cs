using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace NetLpr.Desktop.Converters
{
    public class MultiplyConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null && value is double doubleValue && parameter is string parameterString)
            {
                if (double.TryParse(parameterString, NumberStyles.Float, CultureInfo.InvariantCulture, out double multiplier))
                {
                    return doubleValue * multiplier;
                }
            }
            return 400.0; 
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    
    }
}