using ReactiveUI;

namespace CsGrafeqApplication.ViewModels;

public class ViewModelBase : ReactiveObject
{
    public MultiLanguageResources MultiLanguageResources { get; }=MultiLanguageResources.Instance;
}