using ReactiveUI;

namespace CsGrafeqApplication.Core.ViewModel;

public class KeyboardViewModel : ViewModelBase
{
    public bool IsToggleKeyboardContainerChecked
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
}