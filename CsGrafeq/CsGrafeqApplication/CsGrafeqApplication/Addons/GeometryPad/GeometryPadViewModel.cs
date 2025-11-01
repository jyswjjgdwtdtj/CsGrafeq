using Avalonia.Collections;
using CsGrafeq.Shapes;
using CsGrafeqApplication.ViewModels;

namespace CsGrafeqApplication.Addons.GeometryPad;

internal class GeometryPadViewModel : ViewModelBase
{
    internal AvaloniaList<HasNameActionList> Actions => GeometryActions.Actions;

    internal ShapeList Shapes { get; } = new();

    public EnglishChar Variables => EnglishChar.Instance;
}