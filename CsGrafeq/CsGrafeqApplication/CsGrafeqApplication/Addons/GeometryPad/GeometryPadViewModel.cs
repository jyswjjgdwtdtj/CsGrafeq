using System.Text.Json;
using Avalonia.Collections;
using Avalonia.Styling;
using CsGrafeq.Shapes;
using CsGrafeqApplication.Addons.GeometryPad;
using CsGrafeqApplication.ViewModels;
using ReactiveUI;
using static CsGrafeq.Extension;

namespace CsGrafeqApplication.Addons.GeometryPad;

internal class GeometryPadViewModel : ViewModelBase
{
    public GeometryPadViewModel()
    {
    }

    public string? DebugInfo
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "1";

    internal AvaloniaList<HasNameActionList> Actions=>GeometryActions.Actions;

    internal ShapeList Shapes { get; } = new();

    public EnglishChar Variables => EnglishChar.Instance;
    
}