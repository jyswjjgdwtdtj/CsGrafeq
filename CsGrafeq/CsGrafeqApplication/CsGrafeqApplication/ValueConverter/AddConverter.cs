using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace CsGrafeqApplication.ValueConverter;

public sealed class AddConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count(o=>o is double) == values.Count)
        {
            var vs = values.OfType<double>();
            return vs.Sum();
        }
        return BindingOperations.DoNothing;
    }
}