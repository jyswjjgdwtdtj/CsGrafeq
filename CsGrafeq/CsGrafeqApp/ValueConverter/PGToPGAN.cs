using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using CsGrafeq.Shapes.ShapeGetter;
using CsGrafeqApp.Addons.GeometryPad;

namespace CsGrafeqApp.ValueConverter;

internal class PGToPGAN : IMultiValueConverter
{
    public object? Convert(IList<object?> value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value.Count==2)
        {
            if(value[0] is PointGetter pg&&value[1] is string pa)
                return new PointGetterAndName() { PointGetter = pg, Name = pa };
        }
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}