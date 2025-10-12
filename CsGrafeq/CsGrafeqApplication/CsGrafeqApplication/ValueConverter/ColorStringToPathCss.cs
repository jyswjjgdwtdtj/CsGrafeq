using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CsGrafeqApplication.ValueConverter;

internal class ColorStringToPathCss : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return $"path {{fill:{str}}}";
        }
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}