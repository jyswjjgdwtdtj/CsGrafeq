using System.Globalization;
using Avalonia.Data.Converters;

namespace CsGrafeqApplication.Core.ValueConverter;

internal class DoubleMinus : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double && parameter is string)
        {
            var val = (double)value;
            var param = double.Parse((string)parameter);
            return val - param;
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}