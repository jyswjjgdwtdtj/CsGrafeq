using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;

namespace CsGrafeqApplication.Controls;

[PseudoClasses(":isfirst")]
public class KeyButton : TemplatedControl
{
    public static readonly DirectProperty<KeyButton, string> FirstButtonProperty = AvaloniaProperty.RegisterDirect<KeyButton, string>(
        nameof(FirstButton), o => o.FirstButton, (o, v) => o.FirstButton = v);

    public static readonly DirectProperty<KeyButton, string> SecondButtonProperty = AvaloniaProperty.RegisterDirect<KeyButton, string>(
        nameof(SecondButton), o => o.SecondButton, (o, v) => o.SecondButton = v);

    public static readonly DirectProperty<KeyButton, bool> IsFirstProperty = AvaloniaProperty.RegisterDirect<KeyButton, bool>(
        nameof(IsFirst), o => o.IsFirst, (o, v) => o.IsFirst = v);

    public static readonly DirectProperty<KeyButton, string> CurrentButtonProperty = AvaloniaProperty.RegisterDirect<KeyButton, string>(
        nameof(CurrentButton), o => o.CurrentButton, (o, v) => o.CurrentButton = v);



    public KeyButton()
    {
        PropertyChanged+=OnPropertyChanged;
        IsFirst = true;
        CurrentButton = FirstButton;
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsFirstProperty)
        {
            PseudoClasses.Set(":isfirst", IsFirst);
            CurrentButton=IsFirst?FirstButton:SecondButton;
        }
    }
    
    public string CurrentButton
    {
        get => field;
        set => SetAndRaise(CurrentButtonProperty, ref field, value);
    }

    public bool IsFirst
    {
        get => field;
        set => SetAndRaise(IsFirstProperty, ref field, value);
    }

    public string SecondButton
    {
        get => field;
        set => SetAndRaise(SecondButtonProperty, ref field, value);
    } = "";

    public string FirstButton
    {
        get => field;
        set => SetAndRaise(FirstButtonProperty, ref field, value);
    } = "";
}