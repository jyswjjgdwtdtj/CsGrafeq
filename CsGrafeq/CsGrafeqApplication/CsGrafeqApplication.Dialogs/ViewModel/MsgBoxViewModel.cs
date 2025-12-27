using Avalonia.Controls;
using Avalonia.Media;
using CsGrafeqApplication.Core.ViewModels;

namespace CsGrafeqApplication.Core.Dialogs.ViewModel;

public class MsgBoxViewModel:ViewModelBase
{
    public object? Content { get; set=>this.RaiseAndSetIfChanged(ref field,value); }
    public string Title { get; set=>this.RaiseAndSetIfChanged(ref field,value); }
    public IList<ButtonDefinition> ButtonDefinitions { get; set=>this.RaiseAndSetIfChanged(ref field,value); }
    public bool CanResize { get; }
    public FontFamily FontFamily { get; }
    public WindowIcon WindowIconPath { get; }
    public double MinWidth { get; set; }
    public double MaxWidth { get; set; }
    public double Width { get; set; }
    public double MinHeight { get; set; }
    public double MaxHeight { get; set; }
    public double Height { get; set; }

    public SystemDecorations SystemDecorations { get; set; }
    public bool Topmost { get; set; }

    public SizeToContent SizeToContent { get; set; } = SizeToContent.Height;

    public WindowStartupLocation LocationOfMyWindow { get; }
    
    public bool CloseOnClickAway { get; private set; }
}