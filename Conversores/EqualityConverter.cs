using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace BackupMonitor.Conversores
{
    public class EqualityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            return value.ToString()?.Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
