using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace CsGrafeqApp.ValueConverter;

internal class UIntColorToBrush : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is uint || value is uint?)
        {
            if (value is null || parameter is null)
                return null;
            var val = (uint)value;
            var op = (uint)double.Parse(parameter as string) & 0xff;
            var brush = new SolidColorBrush(val);
            brush.Opacity = (double)op / 0xFF;
            return brush;
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}