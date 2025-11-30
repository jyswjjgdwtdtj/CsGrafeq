using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CsGrafeqApplication.ValueConverter;

public class IIfConverter:IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values?.Count != 3)
            return null;
        if (values[0] is bool con)
        {
            return con ? values[1] : values[2];
        }
        else
        {
            return null;
        }
    }
}