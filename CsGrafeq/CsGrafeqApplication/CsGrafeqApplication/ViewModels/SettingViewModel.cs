using CsGrafeqApplication.Controls.Displayers;
using CsGrafeqApplication.Core.ViewModel;

namespace CsGrafeqApplication.ViewModels;

public class SettingViewModel : ViewModelBase
{
    public Displayer Displayer { get; init; }
}