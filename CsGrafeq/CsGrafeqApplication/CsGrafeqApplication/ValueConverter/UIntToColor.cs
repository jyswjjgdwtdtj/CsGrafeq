using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace CsGrafeqApplication.ValueConverter;

internal class UIntToColor : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is uint)
        {
            var val = (uint)value;
            val &= 0x00FFFFFF;
            val |= 0xFF000000;
            return Color.FromUInt32(val);
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color val)
        {
            return (val.ToUInt32()&0x00FFFFFF)|0xFF000000;;
        }
        return null;
    }
}