using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace CsGrafeqApplication.Core.ValueConverter;

public class SolidColorBrushToColorConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 1 && values[0] is SolidColorBrush scb) return scb.Color;
        return new BindingNotification(new InvalidCastException(),
            BindingErrorType.Error);
    }
}