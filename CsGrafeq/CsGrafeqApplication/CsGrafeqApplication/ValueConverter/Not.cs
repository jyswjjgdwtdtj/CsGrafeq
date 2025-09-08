using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CsGrafeqApplication.ValueConverter;

internal class Not : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool val)
        {
            return !val;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool val)
        {
            return !val;
        }
        return null;
    }
}