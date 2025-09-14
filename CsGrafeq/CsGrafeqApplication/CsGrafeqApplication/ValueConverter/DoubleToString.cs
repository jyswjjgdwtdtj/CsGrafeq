using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CsGrafeqApplication.ValueConverter;

internal class DoubleToString : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return d.ToString();
        }
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            if (double.TryParse(s, out double d))
            {
                return d;
            }
        }
        return null;
    }
}