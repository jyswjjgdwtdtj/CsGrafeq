using Avalonia.Collections;
using CsGrafeq.Shapes;
using CsGrafeqApplication.Core.ViewModel;
using CsGrafeqApplication.ViewModels;
using CSharpMath.Atom;
using ReactiveUI;

namespace CsGrafeqApplication.Addons.GeometryPad;

internal class GeometryPadViewModel : ViewModelBase
{
    internal AvaloniaList<HasNameActionList> Actions => GeometryActions.Actions;

    internal ShapeList Shapes { get; } = new();

    public EnglishChar Variables => EnglishChar.Instance;

    public ImplicitFunction DefaultImplicitFunction { get; } = new("");
}