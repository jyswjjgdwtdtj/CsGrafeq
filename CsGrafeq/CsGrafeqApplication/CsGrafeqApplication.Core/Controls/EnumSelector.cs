using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;

namespace CsGrafeqApplication.Core.Controls;
public class EnumSelector<T> : ContentControl where T : struct,Enum
{
    public ObservableCollection<T> EnumValues { get; } = new();


    public static readonly DirectProperty<EnumSelector<T>, T> EnumValueProperty = AvaloniaProperty.RegisterDirect<EnumSelector<T>, T>(
        nameof(EnumValue), o => o.EnumValue, (o, v) => o.EnumValue = v);

    public T EnumValue
    {
        get => field;
        private set => SetAndRaise(EnumValueProperty, ref field, value);
    }

    public EnumSelector()
    {
        var values = Enum.GetValues<T>();
        for(var i=0;i<values.Length;i++)
        {
            var value = values[i];
            EnumValues.Add(value);
        }
        var combobox=new ComboBox()
        {
            [ItemsControl.ItemsSourceProperty] = EnumValues
        };
        this.GetObservable(EnumValueProperty).Subscribe((o) =>
        {
            combobox.SelectedItem = EnumValue;
        });
        combobox.GetObservable(SelectingItemsControl.SelectedItemProperty).Subscribe((o) =>
            {
                if (o is T t)
                {
                    EnumValue = t;
                }
            });
        Content = combobox;
        
    }
}