using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace CsGrafeqApplication.Core.ValueConverter;

public sealed class AddConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count(o => o is double) == values.Count)
        {
            var vs = values.OfType<double>();
            return vs.Sum();
        }

        return BindingOperations.DoNothing;
    }
}