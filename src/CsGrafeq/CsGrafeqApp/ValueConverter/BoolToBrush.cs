using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace CsGrafeqApp.ValueConverter
{
    internal class BoolToBrush:IValueConverter
    {
        public IBrush Brush1
        {
            get => field;
            set => field = value;
        } = new SolidColorBrush(Colors.Transparent);
        public IBrush Brush2
        {
            get => field;
            set => field = value;
        } = new SolidColorBrush(Colors.Transparent);
        public object? Convert(object? value, Type targetType, object? parameter,CultureInfo culture)
        {
            if(value is bool?||value is bool)
            {
                if (value is null)
                    return null;
                bool bv = (bool)value;
                return bv ? Brush1 : Brush2;
            }
            return null;
        }
        public object ConvertBack(object? value, Type targetType,object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }
}
