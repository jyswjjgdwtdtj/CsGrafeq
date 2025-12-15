using System.Globalization;
using Avalonia.Data.Converters;

namespace CsGrafeqApplication.Core.ValueConverter;

internal class IntToBool : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i) return i != 0;
        if (value is uint ui) return ui != 0;
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}