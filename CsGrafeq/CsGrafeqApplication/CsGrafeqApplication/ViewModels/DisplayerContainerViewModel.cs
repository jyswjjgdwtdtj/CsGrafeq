using CsGrafeqApplication.Controls.Displayers;
using ReactiveUI;

namespace CsGrafeqApplication.ViewModels;

public class DisplayerContainerViewModel : ViewModelBase
{
    public Displayer? Displayer
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    public bool IsOperationVisible
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double SplitterThicknessOff { get; } = OS.GetOSType() == OSType.Android ? 8 : 2;
    public double SplitterThicknessOn { get; } = OS.GetOSType() == OSType.Android ? 8 : 6;
}