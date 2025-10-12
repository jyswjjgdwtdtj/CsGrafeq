using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CsGrafeqApplication.ValueConverter;

internal class BoolToDouble : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && double.TryParse(parameter.ToString(), out var d)) return !b ? d : 0d;
        return 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}