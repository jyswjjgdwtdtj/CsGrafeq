using CsGrafeq.I18N;
using ReactiveUI;

namespace CsGrafeqApplication.Core.ViewModel;

public class ViewModelBase : ReactiveObject
{
    public MultiLanguageResources MultiLanguageResources { get; } = MultiLanguageResources.Instance;
}